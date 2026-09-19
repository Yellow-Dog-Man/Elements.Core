using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System.IO;
using SevenZip;
using LZ4;
using BrotliSharpLib;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices.ComTypes;

namespace Elements.Core
{
    public static class DataTreeConverter
    {
        public static readonly bool NativeBrotliSupported;

        static DataTreeConverter()
        {
            try
            {
                var version = Brotli.Brolib.BrotliDecoderVersion();
                version = Brotli.Brolib.BrotliEncoderVersion();

                NativeBrotliSupported = true;
            }
            catch(Exception ex)
            {
                UniLog.Warning($"Exception from calling native Brotli methods:\n{ex}");
            }
        }

        public const string HEADER = "FrDT";
        public const int VERSION = 0;

        public enum Compression
        {
            None = 0,
            LZ4 = 1,
            LZMA = 2,
            Brotli = 3,
        }

        public static bool IsSupportedFormat(string file)
        {
            var ext = Path.GetExtension(file).ToLower();

            return ext == ".7zbson" || ext == ".lz4bson" || ext == ".brson";
        }

        public static DataTreeDictionary Load(string file, Uri uri)
        {
            var ext = Path.GetExtension(uri.LocalPath).Replace(".", "");
            return Load(file, ext);
        }

        public static DataTreeDictionary Load(string file, string ext = null)
        {
            if(ext == null)
                ext = Path.GetExtension(file).ToLower().Replace(".", "");

            switch(ext)
            {
                case "7zbson":
                    using (var fstream = File.OpenRead(file))
                    {
                        if (TryReadHeader(fstream, out var version, out var compression))
                        {
                            if (version > VERSION)
                                throw new NotSupportedException("Version is too new: " + version);

                            if (compression != Compression.LZMA)
                                throw new InvalidDataException("Expected LZMA compression, but got: " + compression);
                        }
                        else
                            fstream.Seek(0, SeekOrigin.Begin);

                        return FromRaw7zBSON(fstream);
                    }

                case "brson":
                    using (var fstream = File.OpenRead(file))
                    {
                        if (TryReadHeader(fstream, out var version, out var compression))
                        {
                            if (version > VERSION)
                                throw new NotSupportedException("Version is too new: " + version);

                            if (compression != Compression.Brotli)
                                throw new InvalidDataException("Expected Brotli compression, but got: " + compression);
                        }
                        else
                            fstream.Seek(0, SeekOrigin.Begin);

                        return FromRawBRSON(fstream);
                    }

                case "lz4bson":
                    using (var fstream = File.OpenRead(file))
                    {
                        if (TryReadHeader(fstream, out var version, out var compression))
                        {
                            if (version > VERSION)
                                throw new NotSupportedException("Version is too new: " + version);

                            if (compression != Compression.LZ4)
                                throw new InvalidDataException("Expected LZ4 compression, but got: " + compression);
                        }
                        else
                            fstream.Seek(0, SeekOrigin.Begin);

                        return FromRawLZ4BSON(fstream);
                    }

                default:
                    using(var fstream = File.OpenRead(file))
                    {
                        var load = LoadAuto(fstream);

                        if (load != null)
                            return load;
                    }

                    // try to determine the type for legacy files
                    var mime = MimeDetective.MimeTypes.GetFileType(new FileInfo(file));

                    if(mime?.Mime != null)
                    {
                        if (mime.Mime.Contains("lzma"))
                            return Load(file, "7zbson");

                        if (mime.Mime.Contains("lz4"))
                            return Load(file, "lz4");
                    }

                    throw new Exception("Unsupported extension: " + ext);
            }
        }

        public static DataTreeDictionary LoadAuto(Stream stream)
        {
            if (TryReadHeader(stream, out int version, out var compression))
            {
                if (version > VERSION)
                    throw new NotSupportedException("Version too new: " + version);

                switch (compression)
                {
                    case Compression.LZ4:
                        return FromRawLZ4BSON(stream);

                    case Compression.LZMA:
                        return FromRaw7zBSON(stream);

                    case Compression.Brotli:
                        return FromRawBRSON(stream);

                    default:
                        throw new NotImplementedException("Compression not supported: " + compression);
                }
            }

            return null;
        }

        public static void Save(DataTreeDictionary root, string file, Compression compression)
        {
            switch (compression)
            {
                case Compression.LZMA:
                    using (var fstream = File.OpenWrite(file))
                        To7zBSON(root, fstream);
                    return;

                case Compression.LZ4:
                    using (var fstream = File.OpenWrite(file))
                        ToLZ4BSON(root, fstream);
                    return;

                case Compression.Brotli:
                    using (var fstream = File.OpenWrite(file))
                        ToBRSON(root, fstream);
                    return;

                default:
                    throw new Exception("Unsupported compression: " + compression);
            }
        }

        public static DataTreeDictionary FromRawBSON(Stream stream)
        {
            using (var bson = new BsonDataReader(stream))
            {
                bson.CloseInput = false;
                return (DataTreeDictionary)Read(bson);
            }
        }

        public static DataTreeDictionary FromRawLZ4BSON(Stream stream)
        {
            using (var lz = new LZ4Stream(stream, LZ4StreamMode.Decompress))
            using (var bson = new BsonDataReader(lz))
            {
                bson.CloseInput = false;
                return (DataTreeDictionary)Read(bson);
            }
        }

        public static DataTreeDictionary FromRawBRSON(Stream stream)
        {
            // The managed implementation is actually faster at the decompressing than the native one, so just use that instead
            if (NativeBrotliSupported)
            {
                using (var memstream = new MemoryStream())
                {
                    Brotli.BrotliExtensions.DecompressFromBrotli(stream, memstream);

                    memstream.Seek(0, SeekOrigin.Begin);

                    using (var bson = new BsonDataReader(memstream))
                    {
                        bson.CloseInput = false;
                        return (DataTreeDictionary)Read(bson);
                    }
                }
            }
            else
            {
                // Use managed implementation as a backup
                using (var memstream = new MemoryStream())
                {
                    using (var bs = new BrotliStream(stream, System.IO.Compression.CompressionMode.Decompress, true))
                        bs.CopyTo(memstream);

                    memstream.Seek(0, SeekOrigin.Begin);

                    using (var bson = new BsonDataReader(memstream))
                    {
                        bson.CloseInput = false;
                        return (DataTreeDictionary)Read(bson);
                    }
                }
            }
        }

        public static DataTreeDictionary FromRaw7zBSON(Stream stream)
        {
            using (var memstream = new MemoryStream())
            {
                LZMAHelper.Decompress(stream, memstream);
                memstream.Seek(0, SeekOrigin.Begin);

                using (var bson = new BsonDataReader(memstream))
                {
                    bson.CloseInput = false;
                    return (DataTreeDictionary)Read(bson);
                }
            }
        }

        public static void ToRawBSON(DataTreeDictionary root, Stream stream)
        {
            using (var bson = new BsonDataWriter(stream))
            {
                bson.CloseOutput = false;
                Write(root, bson);
            }
        }

        public static void ToRawJSON(DataTreeDictionary root, Stream stream)
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder);

            using (var json = new JsonTextWriter(writer))
            {
                json.Formatting = Formatting.Indented;
                Write(root, json);
            }

            var bw = new BinaryWriter(stream);
            bw.Write(builder.ToString());
            bw.Flush();
        }

        public static void ToLZ4BSON(DataTreeDictionary root, Stream stream)
        {
            WriteHeader(stream, Compression.LZ4);

            using (var lz = new LZ4Stream(stream, LZ4StreamMode.Compress, LZ4StreamFlags.HighCompression))
            using (var bson = new BsonDataWriter(lz))
            {
                bson.CloseOutput = false;
                Write(root, bson);
            }
        }

        public static void ToBRSON(DataTreeDictionary root, Stream stream, int quality = 9)
        {
            WriteHeader(stream, Compression.Brotli);

            if(NativeBrotliSupported)
            {
                using (var memstream = new MemoryStream())
                using (var bson = new BsonDataWriter(memstream))
                {
                    bson.CloseOutput = false;
                    Write(root, bson);

                    memstream.Seek(0, SeekOrigin.Begin);

                    Brotli.BrotliExtensions.CompressToBrotli(memstream, stream, (uint)quality, 16);
                }
            }
            else
            {
                // Use C# implementation as a fallback
                using (var memstream = new MemoryStream())
                using (var bson = new BsonDataWriter(memstream))
                {
                    bson.CloseOutput = false;
                    Write(root, bson);

                    memstream.Seek(0, SeekOrigin.Begin);

                    using (var bs = new BrotliStream(stream, System.IO.Compression.CompressionMode.Compress, true))
                    {
                        bs.SetQuality(quality);
                        bs.SetWindow(16);

                        memstream.CopyTo(bs);
                    }
                }
            }
        }

        public static void To7zBSON(DataTreeDictionary root, Stream stream)
        {
            WriteHeader(stream, Compression.LZMA);

            using (var memstream = new MemoryStream())
            using (var bson = new BsonDataWriter(memstream))
            {
                bson.CloseOutput = false;

                Write(root, bson);

                memstream.Seek(0, SeekOrigin.Begin);
                LZMAHelper.Compress(memstream, stream);
            }
        }

        static void WriteHeader(Stream stream, Compression compression)
        {
            using(var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                for (int i = 0; i < HEADER.Length; i++)
                    writer.Write((byte)HEADER[i]);

                writer.Write(VERSION);
                writer.WriteEnumBinary(compression);
            }
        }

        static bool TryReadHeader(Stream stream, out int version, out Compression compression)
        {
            using(var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                for(int i = 0; i < HEADER.Length; i++)
                {
                    if (reader.ReadByte() != (byte)HEADER[i])
                    {
                        version = -1;
                        compression = default;

                        return false;
                    }
                }

                version = reader.ReadInt32();
                compression = reader.ReadEnumBinary<Compression>();

                return true;
            }
        }

        #region CONVERT TO HELPER FUNCTIONS

        static void Write(DataTreeNode node, JsonWriter writer)
        {
            switch (node)
            {
                case DataTreeValue value:
                    WriteValue(value, writer);
                    break;

                case DataTreeList list:
                    WriteList(list, writer);
                    break;

                case DataTreeDictionary dict:
                    WriteDictionary(dict, writer);
                    break;
            }
        }

        static void WriteValue(DataTreeValue value, JsonWriter writer)
        {
            if (value.IsNull)
                writer.WriteNull();
            else if (value.Value is ulong u64)
                writer.WriteValue(unchecked((long)u64));
            else if (value.Value is uint u32)
                writer.WriteValue(unchecked((int)u32));
            else
                writer.WriteValue(value.Value);
        }

        static void WriteList(DataTreeList list, JsonWriter writer)
        {
            writer.WriteStartArray();

            foreach (var el in list.Children)
                Write(el, writer);

            writer.WriteEndArray();
        }

        static void WriteDictionary(DataTreeDictionary node, JsonWriter writer)
        {
            writer.WriteStartObject();

            foreach(var el in node.Children)
            {
                writer.WritePropertyName(el.Key.ToString());
                Write(el.Value, writer);
            }

            writer.WriteEndObject();
        }

        static DataTreeNode Read(JsonReader reader)
        {
            reader.MaxDepth = null;
            reader.Read();
            return ReadNode(reader);
        }

        static DataTreeNode ReadNode(JsonReader reader)
        {
            switch(reader.TokenType)
            {
                case JsonToken.Boolean:
                    return new DataTreeValue((bool)reader.Value);

                case JsonToken.Float:
                    return new DataTreeValue((double)reader.Value);

                case JsonToken.Integer:
                    return new DataTreeValue((long)reader.Value);

                case JsonToken.String:
                    return DataTreeValue.RawString(reader.Value as string);

                case JsonToken.Date:
                    return new DataTreeValue((DateTime)reader.Value);

                case JsonToken.Null:
                    return new DataTreeValue(null as string);

                case JsonToken.StartArray:
                    return ReadList(reader);

                case JsonToken.StartObject:
                    return ReadDictionary(reader);

                default:
                    return null;
            }
        }

        static DataTreeList ReadList(JsonReader reader)
        {
            var list = new DataTreeList();

            while(reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.EndArray:
                        return list;

                    default:
                        var node = ReadNode(reader);

                        if(node != null)
                            list.Add(node);
                        break;
                }
            }

            throw new Exception("Didn't find end of array!");
        }

        static DataTreeDictionary ReadDictionary(JsonReader reader)
        {
            var dict = new DataTreeDictionary();

            string propertyName = null;

            while(reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.EndObject:
                        return dict;

                    case JsonToken.PropertyName:
                        propertyName = reader.Value as string;
                        break;

                    default:
                        var node = ReadNode(reader);

                        if (node != null)
                        {
                            dict.Add(propertyName, node);
                            propertyName = null;
                        }

                        break;
                }
            }

            throw new Exception("Didn't find end of dictionary!");
        }

        #endregion
    }
}
