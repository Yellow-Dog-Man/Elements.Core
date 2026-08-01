using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elements.Core
{
    public partial class DataTreeDictionary : DataTreeNode, IEnumerable<KeyValuePair<string, DataTreeNode>>
    {
        public override IEnumerable<DataTreeNode> EnumerateTree()
        {
            yield return this;

            foreach (var child in Children)
                foreach (var childNode in child.Value.EnumerateTree())
                    yield return childNode;
        }

        public Dictionary<string, DataTreeNode> Children { get; private set; }

        public DataTreeDictionary()
        {
            Children = new Dictionary<string, DataTreeNode>();
        }

        public void AddOrUpdate(string key, DataTreeNode node)
        {
            // try to remove first if it exists
            Children.Remove(key);
            Children.Add(key, node);
        }

        public void AddOrUpdate<T>(string key, T value)
        {
            Children.Remove(key);
            Add<T>(key, value);
        }

        public bool Remove(string key)
        {
            return Children.Remove(key);
        }

        // Extracts a value or returns a default value
        public bool TryExtract<T>(string key, ref T value)
        {
            if(Children.TryGetValue(key, out DataTreeNode node))
            {
                value = ExtractValue<T>(node);
                return true;
            }
            return false;
        }

        public T ExtractOrDefault<T>(string key, T def = default(T))
        {
            if (Children.TryGetValue(key, out var node))
                return ExtractValue<T>(node);

            return def;
        }

        public T ExtractOrThrow<T>(string key)
        {
            if (Children.TryGetValue(key, out var node))
                return ExtractValue<T>(node);

            throw new Exception($"Value {key} does not exist!");
        }

        public T ExtractOrAdd<T>(string key, T add)
        {
            if (Children.TryGetValue(key, out var node))
                return ExtractValue<T>(node);
            else
            {
                Add<T>(key, add);
                return add;
            }
        }

        public DataTreeNode TryGetNode(string key)
        {
            Children.TryGetValue(key, out var node);

            return node;
        }

        public DataTreeList TryGetList(string key)
        {
            if(Children.TryGetValue(key, out var node))
            {
                if (node is DataTreeList list)
                    return list;
            }

            return null;
        }

        public DataTreeDictionary TryGetDictionary(string key)
        {
            if (Children.TryGetValue(key, out var node))
            {
                if (node is DataTreeDictionary dict)
                    return dict;
            }

            return null;
        }

        public bool ContainsKey(string key) => Children.ContainsKey(key);

        public DataTreeNode this[string key]
        {
            get
            {
                if (Children.TryGetValue(key, out DataTreeNode node))
                    return node;

                throw new Exception("Dictionary doesn't contain key: " + key);
            }
        }

        public IEnumerator<KeyValuePair<string, DataTreeNode>> GetEnumerator() => Children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString()
        {
            var str = new StringBuilder();

            str.AppendLine($"Dictionary {Children.Count}");

            foreach (var child in Children)
                str.AppendLine($"\t{child.Key}: {child.Value}");

            return str.ToString();
        }
    }
}
