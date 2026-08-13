using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Elements.Core
{
    public static class CollectionsExtensions
    {
        public static bool ElementWiseEquals<T>(this IReadOnlyList<T> list, IReadOnlyList<T> other)
            where T : IEquatable<T>
        {
            if (list == null && other == null)
                return true;

            // one of the lists is null, can't equal
            if (list == null || other == null)
                return false;

            if (list.Count != other.Count)
                return false;

            for (int i = 0; i < list.Count; i++)
                if (!list[i].Equals(other[i]))
                    return false;

            return true;
        }

        public static bool IsSame<T>(this HashSet<T> set, HashSet<T> other)
        {
            if (set == null && other == null)
                return true;

            // one of the lists is null, can't equal
            if (set == null || other == null)
                return false;

            if (set.Count != other.Count)
                return false;

            foreach (var element in set)
                if (!other.Contains(element))
                    return false;

            return true;
        }

        public static bool AddUnique<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }

            return false;
        }

        public static T GetOrNull<T>(this IReadOnlyList<T> list, int index)
            where T : class
        {
            if (list.Count <= index)
                return null;
            else
                return list[index];
        }

        public static T GetRandom<T>(this IReadOnlyList<T> list, RandomXGenerator randomSource = null)
        {
            int index = randomSource?.Range(0, list.Count) ?? RandomX.Range(0, list.Count);
            return list[index];
        }

        public static T GetRandomWithWeight<T>(this IReadOnlyList<T> list, Func<T, float> weightGetter)
        {
            float sum = 0;

            for (int i = 0; i < list.Count; i++)
                sum += weightGetter(list[i]);

            var point = RandomX.Range(0, sum);

            sum = 0;

            for(int i = 0; i < list.Count; i++)
            {
                sum += weightGetter(list[i]);
                if (sum >= point)
                    return list[i];
            }

            return list[list.Count - 1];
        }

        public static T TakeRandom<T>(this IList<T> list)
        {
            int index = RandomX.Range(0, list.Count);

            return list.TakeOne(index);
        }

        public static void TakeRandomSet<T>(this IList<T> list, int count, List<T> target)
        {
            for (int i = 0; i < MathX.Min(count, list.Count); i++)
                target.Add(list.TakeRandom());
        }

        public static List<T> TakeRandomSet<T>(this IList<T> list, int count)
        {
            var newList = new List<T>();
            list.TakeRandomSet(count, newList);
            return newList;
        }

        public static T TakeOne<T>(this IList<T> list, int index)
        {
            var t = list[index];
            list.RemoveAt(index);

            return t;
        }

        public static T GetFirst<T>(this IReadOnlyList<T> list)
        {
            return list[0];
        }

        public static T GetLast<T>(this IReadOnlyList<T> list)
        {
            return list[list.Count - 1];
        }

        public static T TakeFirst<T>(this IList<T> list)
        {
            return list.TakeOne(0);
        }

        public static T TakeLast<T>(this IList<T> list)
        {
            return list.TakeOne(list.Count - 1);
        }

        public static void Move<T>(this List<T> list, int from, int to)
        {
            var t = list[from];
            list.RemoveAt(from);

            // adjust the target position, because it was moved by one down
            if (to > from)
                to--;

            list.Insert(to, t);
        }

        public static T FromEnd<T>(this List<T> list, int index)
        {
            return list[list.Count - 1 - index];
        }

        // Fisher-Yates algorithm
        public static void Shuffle<T>(this T[] array, Random random)
        {
            int n = array.Length;

            while (n > 1)
            {
                int k = random.Next(n--);

                var temp = array[n];

                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this T[] array, RandomXGenerator random)
        {
            int n = array.Length;

            while (n > 1)
            {
                int k = random.Range(n--);

                var temp = array[n];

                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static void Shuffle<T>(this List<T> list, Random random)
        {
            int n = list.Count;

            while (n > 1)
            {
                int k = random.Next(n--);

                var temp = list[n];

                list[n] = list[k];
                list[k] = temp;
            }
        }

        public static void EnsureCapacity<T>(this List<T> list, int minCapacity)
        {
            if (list.Capacity < minCapacity)
                list.Capacity = minCapacity;
        }

        public static void ExpandForElements<T>(this List<T> list, int newElementCount)
        {
            list.EnsureCapacity(list.Count + newElementCount);
        }

        public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> predicate)
        {
            for (int i = 0; i < list.Count; i++)
                if (predicate(list[i]))
                    return i;

            return -1;
        }

        public static T[] EnsureSize<T>(this T[] array, int minLength, bool keepData = false)
        {
            return InternalEnsureSize(array, minLength, keepData);
        }

        public static T[] EnsureExactSize<T>(this T[] array, int length, bool keepData = false, bool allowZeroSize = false)
        {
            if (length == 0)
            {
                if (allowZeroSize)
                    return new T[0];
                else
                    return null;
            }

            return InternalEnsureSize(array, length, keepData, true);
        }

        public static void EnsureExactCount<T>(this List<T> collection, int count)
        {
            collection.EnsureMinCount(count);
            collection.EnsureMaxCount(count);
        }

        public static void EnsureExactCount<T>(this IList<T> collection, int count)
        {
            collection.EnsureMinCount(count);
            collection.EnsureMaxCount(count);
        }

        public static void EnsureMinCount<T>(this IList<T> collection, int count)
        {
            while (collection.Count < count)
                collection.Add(default(T));
        }

        public static void EnsureMaxCount<T>(this IList<T> collection, int count)
        {
            while (collection.Count > count)
                collection.RemoveAt(collection.Count - 1);
        }

        public static void EnsureMaxCount<T>(this List<T> collection, int count)
        {
            if (collection.Count <= count)
                return;

            var excess = collection.Count - count;

            collection.RemoveRange(collection.Count - excess, excess);
        }

        static T[] InternalEnsureSize<T>(T[] array, int length, bool keepData = false,
            bool shrinkIfLarge = false)
        {
            if (array == null || array.Length < length || (array.Length > length && shrinkIfLarge))
            {
                var _new = new T[length];
                if (keepData && array != null)
                    Array.Copy(array, _new, Math.Min(array.Length, _new.Length));
                return _new;
            }

            return array;
        }

        public static void ForeachGet<T>(this T[] array, int index, int count, Action<T> action)
        {
            for (int i = index; i < index + count; i++)
                action(array[i]);
        }

        public static void ForeachSet<T>(this T[] array, int index, int count, Func<T> action)
        {
            for (int i = index; i < index + count; i++)
                array[i] = action();
        }

        public static void ForeachProcess<T>(this T[] array, int index, int count, Func<T, T> action)
        {
            for (int i = index; i < index + count; i++)
                array[i] = action(array[i]);
        }

        // Tostring
        public static string ElementsToString<T>(this List<T> list)
        {
            var str = new StringBuilder();

            foreach (var i in list)
                str.AppendLine(i.ToString());

            return str.ToString();
        }

        public static string ElementsToString<T>(this List<T> list, Func<T, string> toString)
        {
            var str = new StringBuilder();

            foreach (var i in list)
                str.AppendLine(toString(i));

            return str.ToString();
        }

        // Arrays

        public static BinaryReader GetBinaryReader(this byte[] buffer)
        {
            return new BinaryReader(new MemoryStream(buffer));
        }

        // Sampling

        public static float Sample(this float[] array, double position)
        {
            long index = MathX.Clamp((long)position, 0, array.Length - 2);
            double ratio = position - index;

            float e0 = array[index];
            float e1 = array[index + 1];

            return (float)(e1 * ratio + e0 * (1 - ratio));
        }

        public static void Replace<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue newValue)
        {
            dict.Remove(key);
            dict.Add(key, newValue);
        }

        // Unsafe fast copy
        public static unsafe void UnsafeCopyTo<A, B>(this A[] source, B[] target, int count)
            where A : struct
            where B : struct
        {
            if (source.Length < count)
                throw new Exception("Soure array doesn't have sufficient number of elements");
            if (target.Length < count)
                throw new Exception("Target array doesn't have sufficient numbre of elements");

            var aSize = Marshal.SizeOf<A>();
            var bSize = Marshal.SizeOf<B>();

            if (aSize != bSize)
                throw new Exception("The source type size doesn't match the destination type size");

            var aHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            var bHandle = GCHandle.Alloc(target, GCHandleType.Pinned);

            var pSource = Marshal.UnsafeAddrOfPinnedArrayElement(source, 0).ToPointer();
            var pTarget = Marshal.UnsafeAddrOfPinnedArrayElement(target, 0).ToPointer();

            Buffer.MemoryCopy(pSource, pTarget, target.Length * bSize, count * aSize);

            aHandle.Free();
            bHandle.Free();
        }

        public static List<List<T>> SplitToGroups<T>(this IEnumerable<T> enumerable, int groupSize)
        {
            var groups = new List<List<T>>();

            foreach (var e in enumerable)
            {
                if (groups.Count == 0 || groups.GetLast().Count == groupSize)
                    groups.Add(new List<T>(groupSize));

                groups.GetLast().Add(e);
            }

            return groups;
        }

        public static T[] Append<T>(this T[] array, T[] other)
        {
            var combined = new T[array.Length + other.Length];

            Array.Copy(array, combined, array.Length);
            Array.Copy(other, 0, combined, array.Length, other.Length);

            return combined;
        }
    }
}
