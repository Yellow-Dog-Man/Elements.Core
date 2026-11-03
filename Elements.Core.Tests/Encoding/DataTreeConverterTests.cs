using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using Renderite.Shared;

namespace Elements.Core.Tests;

[TestClass]
public class DataTreeConverterTests
{
    [DataRow("asset.lz4bson")]
    [DataRow("asset.7zbson")]
    [DataRow("asset.brson")]
    [DataRow("ASSET.LZ4BSON")]
    [TestMethod]
    public void IsSupportedFormat_SupportedFormat_ReturnsTrue(string file)
    {
        Assert.IsTrue(DataTreeConverter.IsSupportedFormat(file));
    }

    [DataRow("asset.bson")]
    [DataRow("asset.json")]
    [DataRow("asset.frdt")]
    [DataRow("asset")]
    [TestMethod]
    public void IsSupportedFormat_UnsupportedFormat_ReturnsFalse(string file)
    {
        Assert.IsFalse(DataTreeConverter.IsSupportedFormat(file));
    }

    [DataRow(DataTreeConverter.Compression.LZ4)]
    [DataRow(DataTreeConverter.Compression.LZMA)]
    [DataRow(DataTreeConverter.Compression.Brotli)]
    [TestMethod]
    public void SaveLoad_CompressionFormat_CompressedTreeEqual(DataTreeConverter.Compression compression)
    {
        var tree = StableKitchenSinkTree().Tree;

        var bytes = Serialize(tree, WriterFor(compression));

        using var stream = new MemoryStream(bytes);
        var newTree = DataTreeConverter.LoadAuto(stream);

        Assert.AreEquivalent(tree, newTree);
    }

    [TestMethod]
    public void LoadAuto_InvalidFormat_ReturnsNull()
    {
        // Enough bytes to fail the header check.
        using var stream = new MemoryStream(new byte[100]);
        var newTree = DataTreeConverter.LoadAuto(stream);

        Assert.IsNull(newTree);
    }

    [TestMethod]
    public void LoadAuto_EmptyStream_ThrowsEndOfStreamException()
    {
        using var stream = new MemoryStream([]);

        Assert.Throws<EndOfStreamException>(() => DataTreeConverter.LoadAuto(stream));
    }

    [TestMethod]
    public void LoadAuto_OverVersionedHeader_ThrowsNotSupportedException()
    {
        var bytes = Encoding.ASCII.GetBytes(DataTreeConverter.HEADER)
            .Concat(BitConverter.GetBytes(int.MaxValue))
            // Pads out the header enough to look like a compression format and data.
            .Concat(new byte[100]).ToArray();
        using var stream = new MemoryStream(bytes);

        Assert.Throws<NotSupportedException>(() => DataTreeConverter.LoadAuto(stream));
    }

    [TestMethod]
    public void LoadAuto_UnknownCompressionFormat_ThrowsNotImplementedException()
    {
        var bytes = Encoding.ASCII.GetBytes(DataTreeConverter.HEADER)
            // Pads out the header enough to look like a compression format and data.
            .Concat(new byte[100]).ToArray();
        using var stream = new MemoryStream(bytes);

        // 0 is a compression format, and the compression format None.
        // This is a bit of a hack, if this test starts failing due to not throwing the proper exception,
        // it may be due to added support for uncompressed files.
        Assert.Throws<NotImplementedException>(() => DataTreeConverter.LoadAuto(stream));
    }

    /// <summary>
    /// This only tests trees that are somewhat shallow (due to AreEquivalent limitations),
    /// and that only include longs. The tree deserializes any int as a long, and it gets detected as a diff.
    /// </summary>
    [DynamicData(nameof(HappyLittleSerialStableTrees))]
    [TestMethod]
    public void SaveLoad_StableTree_TreeSerializesIdentically(TreeCase treeCase)
    {
        var newTree = ResaveTree(treeCase.Tree);

        Assert.AreEquivalent(treeCase.Tree, newTree);
    }

    [DynamicData(nameof(HappyLittleSerializationUnstableTrees))]
    [TestMethod]
    public void SaveLoad_UnstableTree_TreeSerializesLogicallyIdentically(TreeCase treeCase)
    {
        var newTree = ResaveTree(treeCase.Tree);

        AssertUnstableEquivalent(treeCase.Tree, newTree);
    }

    [TestMethod]
    public void SaveLoad_VeryDeeplyNestedTree_TreeSerializesLogicallyIdentically()
    {
        var tree = VeryDeeplyNestedTree().Tree;
        var newTree = ResaveTree(tree);

        AssertUnstableEquivalent(tree, newTree);
    }

    // These two cases are not handled properly by the battery test.
    // It falls back to object equality, which returns true for -0f == 0f.

    [TestMethod]
    public void SaveLoad_NegativeZeroFloat_ExtractsNegZero()
    {
        var dict = ResaveTree(new DataTreeDictionary
        {
            { "NegZero", float.NegativeZero }
        });
        var leaf = (DataTreeValue)dict["NegZero"];

        Assert.IsTrue(float.IsNegative(leaf.Extract<float>()));
    }

    [TestMethod]
    public void SaveLoad_NegativeZeroDouble_ExtractsNegZero()
    {
        var dict = ResaveTree(new DataTreeDictionary
        {
            { "NegZero", double.NegativeZero }
        });
        var leaf = (DataTreeValue)dict["NegZero"];

        Assert.IsTrue(double.IsNegative(leaf.Extract<double>()));
    }

    #region Serialization Unstable Trees

    public static IEnumerable<TreeCase> HappyLittleSerializationUnstableTrees
    {
        get
        {
            yield return KitchenSinkTree();
            yield return NumberLimitsTree();
            yield return VectorTree();
            foreach (var tree in HappyLittleSerialStableTrees)
            {
                yield return tree;
            }
        }
    }

    static TreeCase KitchenSinkTree()
    {
        return new TreeCase("Kitchen Sink", new DataTreeDictionary
        {
            { "Bool", true },
            { "Int", 42 },
            { "Long", -9000000000L },
            { "Float", 1.5f },
            { "Double", -2.25 },
            { "String", "sample" },
            { "NullString", null as string },
            { "Url", new Uri("https://example.com/asset") },
            // NOTE: This stores the enum as a string, a hard-typed extractor is only able to unpack this correctly.
            { "Enum", DataTreeConverter.Compression.LZMA },
            { "float3", new float3(1f, 2f, 3f) },
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
        });
    }

    static TreeCase NumberLimitsTree()
    {
        return new TreeCase("Number limits", new DataTreeDictionary
        {
            { "MinLong", long.MinValue },
            { "MaxLong", long.MaxValue },
            { "MaxUlong", ulong.MaxValue },
            { "MaxUint", uint.MaxValue },
            { "MinInt", int.MinValue },
            { "MaxInt", int.MaxValue },
            { "MinSbyte", sbyte.MinValue },
            { "MaxByte", byte.MaxValue },
            { "MinShort", short.MinValue },
            { "MaxUshort", ushort.MaxValue },
            { "EpsilonFloat", float.Epsilon },
            { "PosInfFloat", float.PositiveInfinity },
            { "NegInfFloat", float.NegativeInfinity },
            { "NanFloat", float.NaN },
            { "MinFloat", float.MinValue },
            { "MaxFloat", float.MaxValue },
            { "EpsilonDouble", double.Epsilon },
            { "PosInfDouble", double.PositiveInfinity },
            { "NegInfDouble", double.NegativeInfinity },
            { "NanDouble", double.NaN },
            { "MinDouble", double.MinValue },
            { "MaxDouble", double.MaxValue },
            { "Pi", System.Math.PI }
        });
    }

    static TreeCase VectorTree()
    {
        return new TreeCase("Vectors", new DataTreeDictionary
        {
            { "float3", new float3(1f, -2.5f, 3.25f) },
            { "double4", new double4(1.0, 2.0, 3.0, 4.0) },
            { "int2", new int2(-5, 7) },
            { "long2", new long2(-5L, 7L) },
            { "colorX", new colorX(0.1f, 0.2f, 0.3f, 0.4f, ColorProfile.Linear) },
            { "floatQ", floatQ.Euler(10f, 20f, 30f) },
            { "doubleQ", doubleQ.Euler(10.0, 20.0, 30.0) }
        });
    }

    #endregion

    #region Serialization Stable Trees

    public static IEnumerable<TreeCase> HappyLittleSerialStableTrees
    {
        get
        {
            yield return StableKitchenSinkTree();
            yield return new TreeCase("Empty Dict", new DataTreeDictionary());
            yield return StringTree();
            yield return DeeplyNestedTree();
            yield return WideTree();
            yield return WideList();
            yield return FrenchTree();
        }
    }

    static TreeCase StableKitchenSinkTree()
    {
        return new TreeCase("Stable Kitchen Sink", new DataTreeDictionary
        {
            { "Bool", true },
            { "Long", -9000000000L },
            { "Double", -2.25 },
            { "String", "sample" },
            { "NullString", null as string },
            { "Url", new Uri("https://example.com/asset") },
            // NOTE: This stores the enum as a string, a hard-typed extractor is only able to unpack this correctly.
            { "StringEnum", DataTreeConverter.Compression.LZMA },
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
        });
    }

    static TreeCase StringTree()
    {
        return new TreeCase("Strings", new DataTreeDictionary
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
    }

    /// <param name="depth">
    /// Assert.AreEquivalent has a limit of 256, but each layer is two properties.
    /// </param>
    static TreeCase DeeplyNestedTree(int depth = 125)
    {
        var root = new DataTreeDictionary();
        var current = root;

        for (long i = 0; i < depth; i++)
        {
            var child = new DataTreeDictionary { { "Depth", i } };

            current.Add("Child", child);
            current = child;
        }

        return new TreeCase("Deeply Nested Tree", root);
    }

    /// <summary>
    /// This tree is a landmine, be very careful using it.
    /// Assert.AreEquivalent does not support this depth.
    /// </summary>
    static TreeCase VeryDeeplyNestedTree()
    {
        // Somewhere between 1000 and 2000 causes a stack overflow on save.
        return new TreeCase("Very Deeply Nested Tree", DeeplyNestedTree(1000).Tree);
    }

    static TreeCase WideTree()
    {
        const int COUNT = 20000;

        var tree = new DataTreeDictionary();

        for (long i = 0; i < COUNT; i++)
        {
            tree.Add("Key" + i, i);
        }

        return new TreeCase("Wide Tree", tree);
    }

    static TreeCase WideList()
    {
        const int COUNT = 20000;

        var tree = new DataTreeList();

        for (long i = 0; i < COUNT; i++)
        {
            tree.Add(new DataTreeValue(i));
        }

        return new TreeCase("Wide List", new DataTreeDictionary
        {
            { "Items", tree }
        });
    }

    static TreeCase FrenchTree()
    {
        return new TreeCase("French", new DataTreeDictionary
        {
            { "qui qui", "🥖" }
        });
    }

    #endregion

    #region Utilities

    static DataTreeDictionary ResaveTree(DataTreeDictionary tree)
    {
        var bytes = Serialize(tree, DataTreeConverter.ToLZ4BSON);
        using var stream = new MemoryStream(bytes);
        return DataTreeConverter.LoadAuto(stream);
    }

    static Action<DataTreeDictionary, Stream> WriterFor(DataTreeConverter.Compression compression)
    {
        return compression switch
        {
            DataTreeConverter.Compression.LZ4 => DataTreeConverter.ToLZ4BSON,
            DataTreeConverter.Compression.LZMA => DataTreeConverter.To7zBSON,
            DataTreeConverter.Compression.Brotli => (root, stream) => DataTreeConverter.ToBRSON(root, stream),
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

    // Gives the test cases names
    public record TreeCase(string Name, DataTreeDictionary Tree)
    {
        public override string ToString() => Name;
    }

    static void AssertUnstableEquivalent(DataTreeNode expected, DataTreeNode actual, string path = "$")
    {
        Assert.IsNotNull(actual, $"{path}: missing node");
        Assert.AreEqual(expected.GetType(), actual.GetType(), $"{path}: node type mismatch");

        switch (expected)
        {
            case DataTreeValue expectedValue:
                AssertNormalizedEqual(expectedValue, (DataTreeValue)actual, path);
                break;

            case DataTreeList expectedList:
                var actualList = (DataTreeList)actual;

                Assert.AreEqual(expectedList.Count, actualList.Count, $"{path}: list length mismatch");

                for (var i = 0; i < expectedList.Count; i++)
                {
                    AssertUnstableEquivalent(expectedList[i], actualList[i], $"{path}/{i}");
                }

                break;

            case DataTreeDictionary expectedDict:
                var actualDict = (DataTreeDictionary)actual;

                Assert.AreEqual(expectedDict.Children.Count, actualDict.Children.Count,
                    $"{path}: dictionary size mismatch");

                foreach (var pair in expectedDict.Children)
                {
                    Assert.IsTrue(actualDict.ContainsKey(pair.Key), $"{path}: missing key {pair.Key}");
                    AssertUnstableEquivalent(pair.Value, actualDict[pair.Key], $"{path}/{pair.Key}");
                }

                break;

            default:
                Assert.Fail($"{path}: Unexpected node type {expected}, this is likely a test issue.");
                break;
        }
    }

    static void AssertNormalizedEqual(DataTreeValue expected, DataTreeValue actual, string path)
    {
        if (expected.Value == null)
        {
            Assert.IsNull(actual.Value, $"{path}: Expected null value");
            Assert.IsTrue(actual.IsNull, $"{path}: Expected value to be marked as null");
            return;
        }

        Assert.IsNotNull(actual.Value, $"{path}: Expected value to be non-null");

        if (expected.Value is string && expected.IsURL)
        {
            // Both are strings, just compare it.
            Assert.AreEqual(expected.TryExtractURL(), actual.TryExtractURL(), $"{path}: value mismatch");
            Assert.IsTrue(actual.IsURL, $"{path}: value mismatch");
            return;
        }

        // Urls were already checked, if one snuck by it was unexpected.
        Assert.IsFalse(actual.IsURL, $"{path}: Unexpected URL value");

        // ulong values from an in-memory data tree don't extract correctly right now.
        // TODO: Remove this if Elements.Core.Tests.DataTreeValueTests.Extract_InMemoryULong_ExtractsValue is fixed.
        if (expected.Value is ulong)
        {
            Assert.AreEqual(expected.Value, actual.Extract<ulong>(), $"{path}: value mismatch");
            return;
        }

        var extractor = ExtractCache.GetOrAdd(expected.Value.GetType(), MakeExtractionMethod);

        var expectedExtracted = extractor(expected);
        var actualExtracted = extractor(actual);

        Assert.AreEqual(expectedExtracted, actualExtracted, $"{path}: value mismatch");
    }

    static readonly ConcurrentDictionary<Type, Func<object, object>> ExtractCache = new();

    static Func<object, object> MakeExtractionMethod(Type type)
    {
        // This is sketchy, but work with me...
        // The extract method does type conversions that are non-trivial.
        // We don't care how they're done, just that they're 1-1.
        var method = typeof(DataTreeValue).GetMethod(nameof(DataTreeValue.Extract), types: [])!
            .MakeGenericMethod(type);
        return obj =>
        {
            try
            {
                return method.Invoke(obj, []);
            }
            catch (TargetInvocationException e)
            {
                // If extract throws an error, raise it instead of the reflection-wrapped error.
                if (e.InnerException != null)
                {
                    throw e.InnerException;
                }

                throw;
            }
        };
    }

    #endregion
}
