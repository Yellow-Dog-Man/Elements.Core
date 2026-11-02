namespace Elements.Core.Tests;

[TestClass]
public class DataTreeNodeTests
{
    // WARNING: This test may break if the dictionaries in DataTreeDictionary reorder their keys.
    // If this test fails randomly, just ensure the iteration is some form of depth-first.
    [TestMethod]
    public void EnumerateTree_MultiLevelTree_MatchesKnownEnumerationOrder()
    {
        var nodes = DataTreeHelpers.MakeNumericTree();

        DataTreeHelpers.ValidateNumericTree(nodes);
    }
}
