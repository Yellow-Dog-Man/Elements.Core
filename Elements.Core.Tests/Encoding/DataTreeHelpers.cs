using BepuPhysics.Trees;

namespace Elements.Core.Tests;

public static class DataTreeHelpers
{
    public static DataTreeDictionary MakeNumericTree()
    {
        return new DataTreeDictionary
        {
            { "a", 1 },
            {
                "b", new DataTreeList
                {
                    new DataTreeDictionary
                    {
                        { "a", 2 },
                        { "b", 3 },
                        { "c", 4 }
                    },
                    new DataTreeDictionary
                    {
                        { "a", 5 },
                        {
                            "b", new DataTreeList
                            {
                                new DataTreeDictionary
                                {
                                    { "a", 6 },
                                    { "b", 7 },
                                    { "c", 8 }
                                },
                                new DataTreeDictionary
                                {
                                    { "a", 9 },
                                    { "b", 10 },
                                    { "c", 11 }
                                }
                            }
                        },
                        { "c", 12 },
                        { "d", 13 },
                    },
                    new DataTreeDictionary
                    {
                        { "a", 14 },
                        { "b", 15 },
                        { "c", 16 }
                    }
                }
            },
            { "c", 17 },
            {
                "d",
                new DataTreeDictionary
                {
                    { "a", 18 },
                    { "b", 19 },
                    { "c", 20 }
                }
            },
            { "e", 21 },
            { "f", 22 },
        };
    }

    public static void ValidateNumericTree(DataTreeDictionary dictionary)
    {
        var numbers = dictionary.EnumerateTree().OfType<DataTreeValue>().Select(v => v.LoadInt()).ToArray();
        var expected = Enumerable.Range(1, 22).ToArray();

        Assert.AreSequenceEqual(expected, numbers);
    }
}
