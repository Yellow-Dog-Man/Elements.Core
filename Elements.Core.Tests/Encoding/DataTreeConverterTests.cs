using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Elements.Core.Tests;

[TestClass]
public class DataTreeConverterTests
{
    [DataRow("asset.lz4bson", true)]
    [DataRow("asset.7zbson", true)]
    [DataRow("asset.brson", true)]
    [DataRow("ASSET.LZ4BSON", true)]
    [DataRow("asset.bson", false)]
    [DataRow("asset.json", false)]
    [DataRow("asset.frdt", false)] // TODO: This should be true, and is probably a bug.
    [DataRow("asset", false)]
    [TestMethod]
    public void IsSupportedFormatRecognizesExtensions(string file, bool expected)
    {
        Assert.AreEqual(expected, DataTreeConverter.IsSupportedFormat(file));
    }

    [DataRow(DataTreeConverter.Compression.LZ4)]
    [DataRow(DataTreeConverter.Compression.LZMA)]
    [DataRow(DataTreeConverter.Compression.Brotli)]
    [TestMethod]
    public void CompressionStreamIntegrity(DataTreeConverter.Compression compression)
    {
        var tree = BuildSampleTree();

        var bytes = Serialize(tree, WriterFor(compression));

        using var stream = new MemoryStream(bytes);
        var newTree = DataTreeConverter.LoadAuto(stream);
        Assert.AreEquivalent(tree, newTree);
    }

    // Gives the test cases names
    public record TreeCase(string Name, DataTreeDictionary Tree)
    {
        public override string ToString() => Name;
    }

    public static IEnumerable<TreeCase> HappyLittleTrees
    {
        get
        {
            // Commented types do not come through as the same type, ints are promoted to longs, floats to doubles.
            yield return new TreeCase("Kitchen Sink", BuildSampleTree());
            yield return new TreeCase("Empty", new DataTreeDictionary());
            yield return new TreeCase("Number Limits", new DataTreeDictionary
            {
                { "MinLong", long.MinValue },
                { "MaxLong", long.MaxValue },
                //{ "MaxUlong", ulong.MaxValue },
                //{ "MaxUint", uint.MaxValue },
                //{ "MinInt", int.MinValue },
                //{ "MaxInt", int.MaxValue },
                //{ "MinSbyte", sbyte.MinValue },
                //{ "MaxByte", byte.MaxValue },
                //{ "MinShort", short.MinValue },
                //{ "MaxUshort", ushort.MaxValue },
                //{ "Epsilon", float.Epsilon },
                //{ "MinFloat", float.MinValue },
                //{ "MaxFloat", float.MaxValue },
                { "Epsilon", double.Epsilon },
                { "MinDouble", double.MinValue },
                { "MaxDouble", double.MaxValue },
                { "NegativeZero", -0.0 },
                { "Pi", System.Math.PI }
            });
            yield return new TreeCase("Strings", new DataTreeDictionary
            {
                { "Empty", "" },
                { "Ascii", "hello world" },
                { "Unicode", "こんにちは ☃ 😀" },
                { "Embedded", "line1\nline2\ttabbed\0nul" },
                { "Quotes", "he said \"hi\" and 'bye' \\ /" },
                { "AtPrefixed", "@not-a-url" },
                { "DoubleAt", "@@literal" },
                { "Null", null as string }
            });
            yield return new TreeCase("Vectors", new DataTreeDictionary
            {
                //{ "float3", new float3(1f, -2.5f, 3.25f) },
                { "double4", new double4(1.0, 2.0, 3.0, 4.0) },
                //{ "int2", new int2(-5, 7) },
                { "long2", new long2(-5L, 7L) },
                //{ "colorX", new colorX(0.1f, 0.2f, 0.3f, 0.4f, ColorProfile.Linear) },
                //{ "floatQ", floatQ.Euler(10f, 20f, 30f) },
                { "doubleQ", doubleQ.Euler(10.0, 20.0, 30.0) }
            });
            yield return new TreeCase("Deep", DeeplyNestedTree());
            yield return new TreeCase("Wide Tree", WideTree());
            yield return new TreeCase("Wide List", WideList());
            yield return new TreeCase("French", new DataTreeDictionary
            {
                { "qui qui", "🥖" }
            });
        }
    }

    [DynamicData(nameof(HappyLittleTrees))]
    [TestMethod]
    public void RoundTripCannedTree(TreeCase tree)
    {
        // Just use one kind of wrapper; this is not a test of the stream layer.
        var bytes = Serialize(tree.Tree, DataTreeConverter.ToLZ4BSON);
        using var stream = new MemoryStream(bytes);
        var newTree = DataTreeConverter.LoadAuto(stream);
        Assert.AreEquivalent(tree.Tree, newTree);
    }

    static Action<DataTreeDictionary, Stream> WriterFor(DataTreeConverter.Compression compression)
    {
        return compression switch
        {
            DataTreeConverter.Compression.LZ4 => DataTreeConverter.ToLZ4BSON,
            DataTreeConverter.Compression.LZMA => DataTreeConverter.To7zBSON,
            DataTreeConverter.Compression.Brotli => (root, stream) => DataTreeConverter.ToBRSON(root, stream),
            DataTreeConverter.Compression.None => throw new NotSupportedException("Compression required"),
            _ => throw new NotSupportedException("No writer for " + compression)
        };
    }

    static byte[] Serialize(DataTreeDictionary root, Action<DataTreeDictionary, Stream> writer)
    {
        using var stream = new MemoryStream();
        writer(root, stream);

        // ToArray works even if the writer closed the stream
        return stream.ToArray();
    }

    static DataTreeDictionary BuildSampleTree()
    {
        // Commented types do not come through as the same type, ints are promoted to longs, floats to bools.
        return new DataTreeDictionary
        {
            { "Bool", true },
            //{ "Int", 42 },
            { "Long", -9000000000L },
            //{ "Float", 1.5f },
            { "Double", -2.25 },
            { "String", "sample" },
            { "NullString", null as string },
            { "Url", new Uri("https://example.com/asset") },
            { "Enum", DataTreeConverter.Compression.LZMA },
            //{ "float3", new float3(1f, 2f, 3f) },
            { "double3", new double3(1.0, 2.0, 3.0) },
            {
                "List", new DataTreeList
                {
                    new DataTreeValue(1L),
                    DataTreeValue.RawString("two"),
                    new DataTreeValue(null as string),
                    new DataTreeDictionary { { "Nested", "value" } },
                    new DataTreeList { new DataTreeValue(3.5) }
                }
            },
            {
                "Child", new DataTreeDictionary
                {
                    { "ChildKey", "childValue" },
                    { "ChildList", new DataTreeList() }
                }
            }
        };
    }

    static DataTreeDictionary DeeplyNestedTree()
    {
        // Assert.AreEquivalent has a limit of 256, but each layer is two properties.
        const int DEPTH = 125;
        var root = new DataTreeDictionary();
        var current = root;

        for (long i = 0; i < DEPTH; i++)
        {
            var child = new DataTreeDictionary { { "Depth", i } };

            current.Add("Child", child);
            current = child;
        }

        return root;
    }

    static DataTreeDictionary WideTree()
    {
        const int COUNT = 20000;

        var tree = new DataTreeDictionary();

        for (long i = 0; i < COUNT; i++)
        {
            tree.Add("Key" + i, i);
        }

        return tree;
    }

    static DataTreeDictionary WideList()
    {
        const int COUNT = 20000;

        var tree = new DataTreeList();

        for (long i = 0; i < COUNT; i++)
        {
            tree.Add(new DataTreeValue(i));
        }

        return new DataTreeDictionary
        {
            { "Items", tree }
        };
    }

    [TestMethod]
    public void EnumerateKnownChildSequence()
    {
        var node = new DataTreeDictionary
        {
            { "l0", new DataTreeList { new DataTreeValue(0) } },
            { "A", 1 },
            { "B", 2 },
            {
                "n1", new DataTreeDictionary
                {
                    { "C", 3 },
                    { "D", 4 },
                }
            },
            { "E", 5 },
            {
                "l1", new DataTreeList
                {
                    new DataTreeValue(6), new DataTreeDictionary
                    {
                        { "F", 7 },
                        { "G", 8 },
                    },
                    new DataTreeValue(9),
                }
            },
            { "D", 10 },
        };

        var order = node.EnumerateTree()
            .OfType<DataTreeValue>().Select(v => (int)v.Value).ToArray();
        
        CollectionAssert.AreEqual(Enumerable.Range(0, 11).ToArray(), order);
    }
}
