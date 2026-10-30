using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerationConfig
{
    public class TypeInfo
    {
        public readonly TypeInfo BaseType;
        public readonly string TypeName;
        public readonly string TypeDeclaration;
        public readonly string BinaryReaderName;
        public readonly int Dimensions;
        public readonly MatrixSize MatrixSize;
        public readonly Type Type;

        public bool IsVector => Dimensions > 1 && !IsMatrix;
        public bool IsQuaternion => TypeDeclaration.EndsWith("Q");
        public bool IsColor => TypeDeclaration.StartsWith("color");
        public bool IsMatrix => MatrixSize != null && (MatrixSize.Columns > 0 && MatrixSize.Rows > 0);

        public TypeInfo(string typeName, string typeDeclaration, string binaryReaderName,
            TypeInfo baseType = null, int dimensions = 1, MatrixSize matrixSize = default(MatrixSize),
             Type type = null)
        {
            this.TypeName = typeName;
            this.Type = type;
            this.TypeDeclaration = typeDeclaration;
            this.BinaryReaderName = binaryReaderName;
            this.Dimensions = dimensions;
            this.BaseType = baseType;
            this.MatrixSize = matrixSize;
        }

        public override string ToString() => TypeDeclaration;
    }

    public class MatrixSize : IEquatable<MatrixSize>
    {
        public readonly int Rows;
        public readonly int Columns;

        public MatrixSize(int rows, int columns)
        {
            this.Rows = rows;
            this.Columns = columns;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MatrixSize)obj);
        }

        public bool Equals(MatrixSize other) => other != null && Rows == other.Rows && Columns == other.Columns;

        public static bool operator ==(MatrixSize left, MatrixSize right)
        {
            if (left is null && right is null)
                return true;

            if(left is null || right is null)
                return false;

            return left.Equals(right);
        }

        public static bool operator !=(MatrixSize left, MatrixSize right) => !(left == right);
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (Rows * 397) ^ Columns;
            }
        }
    }

    public static class Config
    {
        public static TypeInfo ObjectType => new TypeInfo("System.Object", "object", null, type: typeof(object));
        public static TypeInfo StringType => new TypeInfo("System.String", "string", null, type: typeof(string));
        public static TypeInfo CharType => new TypeInfo("System.Char", "char", null, type: typeof(char));

        public static List<TypeInfo> PrimitiveTypes { get; }

        public static List<TypeInfo> VectorBaseTypes { get; }
        public static List<TypeInfo> MatrixBaseTypes { get; }
        public static List<TypeInfo> QuaternionBaseTypes { get; }

        public static List<MatrixSize> MatrixSizes { get; }

        public static List<TypeInfo> VectorTypes { get; }
        public static List<TypeInfo> MatrixTypes { get; }
        public static List<TypeInfo> QuaternionTypes { get; }

        public static List<TypeInfo> AllSupportedTypes { get; }

        public static Dictionary<string, TypeInfo> TypeInfos { get; }

        public static string VectorElements => "xyzwv";

        static string[] primitives =
        {
            "bool",
            "byte", "ushort", "uint", "ulong",
            "sbyte", "short", "int", "long",
            "float", "double", "decimal",
            "char", "string", "Uri",
        };

        static string[] primitiveReadFunctions =
        {
            "Boolean",
            "Byte", "UInt16", "UInt32", "UInt64",
            "SByte", "Int16", "Int32", "Int64",
            "Single", "Double", "Decimal",
            "Char", "String", "Uri",
        };

        static Type[] primitiveTypes =
        {
            typeof(bool),
            typeof(byte), typeof(ushort), typeof(uint), typeof(ulong),
            typeof(sbyte), typeof(short), typeof(int), typeof(long),
            typeof(float), typeof(double), typeof(decimal),
            typeof(char), typeof(string), typeof(Uri)
        };

        static Config()
        {
            TypeInfos = new Dictionary<string, TypeInfo>();

            // build primitive types first
            List<TypeInfo> baseTypes = new List<TypeInfo>();

            for(int i = 0; i < primitives.Length; i++)
            {
                string declaration = primitives[i];

                var typeinfo = new TypeInfo(H.Capitalize(declaration), declaration, primitiveReadFunctions[i],
                    type: primitiveTypes[i]);

                baseTypes.Add(typeinfo);

                TypeInfos.Add(declaration, typeinfo);
            }

            PrimitiveTypes = baseTypes;

            // Setup base vector types
            VectorBaseTypes = CollectTypeInfos("bool", "float", "double", "byte", "sbyte", "ushort", "short", "int", "uint", "long", "ulong");

            VectorTypes = new List<TypeInfo>();

            // build vector type infos
            for(int n = 2; n <= 4; n++)
            {
                foreach(var type in baseTypes)
                {
                    // skip those types since vectors don't make much sense
                    if (type.TypeName == "String" ||  type.TypeName == "Char" || type.TypeName == "Uri")
                        continue;

                    var typeinfo = new TypeInfo(type.TypeName + n, type.TypeDeclaration + n,
                        n + "D_" + type.BinaryReaderName, type, n);

                    TypeInfos.Add(typeinfo.TypeDeclaration, typeinfo);

                    if (VectorBaseTypes.Contains(type))
                        VectorTypes.Add(typeinfo);
                }
            }

            // Setup base matrix types
            MatrixBaseTypes = CollectTypeInfos("float", "double");

            // Setup matrix sizes
            MatrixSizes = new List<MatrixSize>();
            for(int i = 2; i <= 4; i++)
                MatrixSizes.Add(new MatrixSize(i, i));

            MatrixTypes = new List<TypeInfo>();

            // build matrix type infos
            foreach (var size in MatrixSizes)
            {
                foreach(var type in baseTypes)
                {
                    if (type.TypeName == "String" || type.TypeName == "Char" || type.TypeName == "Bool" || type.TypeName == "Uri")
                        continue;

                    string sizename = size.Rows + "x" + size.Columns;

                    var typeinfo = new TypeInfo(type.TypeName + sizename, type.TypeDeclaration + sizename,
                        sizename + "Matrix" + type.BinaryReaderName, type, size.Rows * size.Columns, size);

                    TypeInfos.Add(typeinfo.TypeDeclaration, typeinfo);

                    if (MatrixBaseTypes.Contains(type))
                        MatrixTypes.Add(typeinfo);
                }
            }

            // Setup Quaternion base types
            QuaternionBaseTypes = CollectTypeInfos("float", "double");

            QuaternionTypes = new List<TypeInfo>();

            // build quaternion type infos
            foreach(var type in baseTypes)
            {
                if (type.TypeName == "String" || type.TypeName == "Char" || type.TypeName == "Bool" || type.TypeName == "Uri")
                    continue;

                var typeinfo = new TypeInfo(type.TypeName + "Q", type.TypeDeclaration + "Q",
                    type.BinaryReaderName + "Q", type, 4);

                TypeInfos.Add(typeinfo.TypeDeclaration, typeinfo);

                if (QuaternionBaseTypes.Contains(type))
                    QuaternionTypes.Add(typeinfo);
            }

            // Create DateTime type
            var datetimetype = new TypeInfo("DateTime", "System.DateTime", "DateTime", type: typeof(DateTime));
            var timespantype = new TypeInfo("TimeSpan", "System.TimeSpan", "TimeSpan", type: typeof(TimeSpan));

            // Create Color type
            var colortype = new TypeInfo("Color", "color", "Color", GetTypeInfo("float"), 4);
            // Treat this as a composite type and pass other functionality to the underlying Color struct where possible
            var colorxtype = new TypeInfo("ColorX", "colorX", "ColorX", GetTypeInfo("float"), 4);

            // Create Color32 type
            var color32type = new TypeInfo("Color32", "color32", "Color32", GetTypeInfo("byte"), 4);

            // Collect all supported types
            AllSupportedTypes = new List<TypeInfo>();

            AllSupportedTypes.AddRange(PrimitiveTypes);
            AllSupportedTypes.AddRange(VectorTypes);
            AllSupportedTypes.AddRange(MatrixTypes);
            AllSupportedTypes.AddRange(QuaternionTypes);

            AllSupportedTypes.Add(datetimetype);
            AllSupportedTypes.Add(timespantype);

            AllSupportedTypes.Add(colortype);
            AllSupportedTypes.Add(colorxtype);
            AllSupportedTypes.Add(color32type);
        }

        static List<TypeInfo> CollectTypeInfos(params string[] types)
        {
            List<TypeInfo> list = new List<TypeInfo>();

            foreach (var type in types)
                list.Add(TypeInfos[type]);

            return list;
        }

        public static TypeInfo GetTypeInfo(string declaration)
        {
            return TypeInfos[declaration];
        }
    }
}
