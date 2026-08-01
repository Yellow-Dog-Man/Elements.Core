#nullable enable
using System.Collections.Generic;

namespace Elements.Core;

public abstract class DataTreeNode
{
    /// <summary>
    /// Returns the direct children of this node, or null if it has no children.
    /// Returning an empty enumerable is acceptable, but may consume more memory.
    /// </summary>
    protected abstract IEnumerable<DataTreeNode>? DirectChildren();

    public IEnumerable<DataTreeNode> EnumerateTree()
    {
        yield return this;
        
        var visits = new Stack<IEnumerator<DataTreeNode>>();

        try
        {
            PushChildrenToVisit(this);
        
            while (visits.TryPeek(out var enumerator))
            {
                if (enumerator.MoveNext())
                {
                    var child = enumerator.Current;
                    yield return child;
                    PushChildrenToVisit(child);
                }
                else
                {
                    visits.Pop().Dispose();
                }
            }
        }
        finally
        {
            while (visits.TryPop(out var e)) e.Dispose();
        }

        yield break;

        void PushChildrenToVisit(DataTreeNode node)
        {
            // Enumerator is disposed when it is popped from the stack.
            // ReSharper disable once GenericEnumeratorNotDisposed
            var enumerator = node.DirectChildren()?.GetEnumerator();
            if (enumerator != null)
            {
                visits.Push(enumerator);
            }
        }
    }
}