namespace Elements.Core.Tests;

[TestClass]
public class DataTreeCompressionRoundTripTests
{
    [TestMethod]
    public void NoCompression()
    {
        var nodes = DataTreeHelpers.MakeNumericTree();

        using var stream = new MemoryStream();

        DataTreeConverter.ToRawBSON(nodes, stream);

        // Seek back
        stream.Seek(0, SeekOrigin.Begin);

        var decodedNodes = DataTreeConverter.FromRawBSON(stream);

        DataTreeHelpers.ValidateNumericTree(decodedNodes);
    }

    [TestMethod]
    public void BrotliCompression()
    {
        var nodes = DataTreeHelpers.MakeNumericTree();

        using var stream = new MemoryStream();

        DataTreeConverter.ToBRSON(nodes, stream);

        // Seek back
        stream.Seek(0, SeekOrigin.Begin);

        var decodedNodes = DataTreeConverter.LoadAuto(stream);

        DataTreeHelpers.ValidateNumericTree(decodedNodes);
    }

    [TestMethod]
    public void LZ4_Compression()
    {
        var nodes = DataTreeHelpers.MakeNumericTree();

        using var stream = new MemoryStream();

        DataTreeConverter.ToLZ4BSON(nodes, stream);

        // Seek back
        stream.Seek(0, SeekOrigin.Begin);

        var decodedNodes = DataTreeConverter.LoadAuto(stream);

        DataTreeHelpers.ValidateNumericTree(decodedNodes);
    }

    [TestMethod]
    public void LZMA_Compression()
    {
        var nodes = DataTreeHelpers.MakeNumericTree();

        using var stream = new MemoryStream();

        DataTreeConverter.To7zBSON(nodes, stream);

        // Seek back
        stream.Seek(0, SeekOrigin.Begin);

        var decodedNodes = DataTreeConverter.LoadAuto(stream);

        DataTreeHelpers.ValidateNumericTree(decodedNodes);
    }
}
