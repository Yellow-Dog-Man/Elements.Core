using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public class DataTreeList : DataTreeNode, IEnumerable<DataTreeNode>
    {
        protected override IEnumerable<DataTreeNode> DirectChildren() => Children.Count > 0 ? Children : null;

        public int Count => Children.Count;

        public List<DataTreeNode> Children { get; private set; }

        public DataTreeList()
        {
            Children = new List<DataTreeNode>();
        }

        public void Add(DataTreeNode node)
        {
            Children.Add(node);
        }

        public DataTreeNode this[int index]
        {
            get { return Children[index]; }
        }

        public IEnumerator<DataTreeNode> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            var str = new StringBuilder();

            str.AppendLine($"List {Children.Count}");

            foreach (var child in Children)
                str.AppendLine($"\t{child}");

            return str.ToString();
        }
    }
}
