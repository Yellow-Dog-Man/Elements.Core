namespace Elements.Core.Tests;

[TestClass]
public class DataTreeValueTests
{
    /// <summary>
    /// The enum is stored as a string, the battery tests extract it as one.
    /// This hard-typed extractor emulates codegen extractors.
    /// </summary>
    [TestMethod]
    public void ExtractEnum_EnumProvided_ExtractsHardTypedValue()
    {
        var expected = DataTreeConverter.Compression.Brotli;
        var dict = new DataTreeDictionary
        {
            { "Enum", expected }
        };
        var leaf = (DataTreeValue)dict["Enum"];

        Assert.AreEqual(expected, leaf.ExtractEnum<DataTreeConverter.Compression>());
    }

    [TestMethod,
     Ignore(
         "On load from wire values are loaded as longs instead of ulongs, the case of loading an in-memory ulong was missed and fails.")]
    public void Extract_InMemoryULong_ExtractsValue()
    {
        var dict = new DataTreeDictionary
        {
            { "MaxUlong", ulong.MaxValue }
        };
        var leaf = (DataTreeValue)dict["MaxUlong"];

        Assert.AreEqual(ulong.MaxValue, leaf.Extract<ulong>());
    }
}
