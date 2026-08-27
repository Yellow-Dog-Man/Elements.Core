using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    /// <summary>
    /// This wraps an IReadOnlyList<T> input in order to provide safety against explicit casts to non-readonly types
    /// It also has a getter for the underlying list, which allows to wrap dynamically changing list fields.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReadOnlyListWrapper<T> : IReadOnlyList<T>
    {
        IReadOnlyList<T> Current => listGetter();

        readonly Func<IReadOnlyList<T>> listGetter;

        public ReadOnlyListWrapper(IReadOnlyList<T> list)
        {
            ArgumentNullException.ThrowIfNull(list);

            listGetter = () => list;
        }

        public ReadOnlyListWrapper(Func<IReadOnlyList<T>> listGetter)
        {
            ArgumentNullException.ThrowIfNull(listGetter);

            this.listGetter = listGetter;
        }

        public int Count => Current.Count;
        public T this[int index] => Current[index];
        public IEnumerator<T> GetEnumerator() => Current.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Current).GetEnumerator();
    }
}
