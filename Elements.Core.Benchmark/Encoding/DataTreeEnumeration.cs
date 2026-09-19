namespace Elements.Core.Benchmark.Encoding;

public class DataTreeEnumeration
{
    [ParamsSource(nameof(Nodes))]
    public DataTreeNode Node { get; set; }
    public IEnumerable<DataTreeNode> Nodes => [MakeDeepTree(), MakeDeepList()]; 
    
    static DataTreeDictionary MakeDeepTree()
    {
        var root = new DataTreeDictionary();
        var current = root;
        
        for (int i = 0; i < 1000; i++)
        {
            var leaf = new DataTreeDictionary();
            current.Add($"{i}", leaf);
            current = leaf;
        }

        return root;
    }

    static DataTreeList MakeDeepList()
    {
        var root = new DataTreeList();
        var current = root;

        for (int i = 0; i < 1000; i++)
        {
            var leaf = new DataTreeList();
            current.Add(leaf);
            current = leaf;
        }

        return root;
    }

    [Benchmark]
    public List<DataTreeNode> IterateAllChildren()
    {
        return Node.EnumerateTree().ToList();
    }
}
