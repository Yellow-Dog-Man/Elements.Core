using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public static class StringSegmentHelper
    {
        public static void Append(this StringBuilder builder, StringSegment segment)
            => builder.Append(segment.str, segment.offset, segment.length);
        public static void AppendLine(this StringBuilder builder, StringSegment segment)
        {
            builder.Append(segment.str, segment.offset, segment.length);
            builder.AppendLine();
        }
    }

    public readonly struct StringSegment
    {
        public readonly string str;
        public readonly int offset;
        public readonly int length;

        public bool IsEmpty => length == 0;

        public StringSegment(string str)
        {
            this.str = str;
            this.offset = 0;
            this.length = str.Length;
        }

        public StringSegment(string str, int offset, int length)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("Offset cannot be negative");

            if (offset > str.Length)
                throw new ArgumentOutOfRangeException("Offset cannot be at the end of the string");

            if (length < 0)
                throw new ArgumentOutOfRangeException("Length cannot be negative");

            if (offset + length > str.Length)
                throw new ArgumentOutOfRangeException("Length cannot be longer than the substring");

            this.str = str;
            this.offset = offset;
            this.length = length;
        }

        public char this[int i]
        {
            get
            {
                if (i > length)
                    throw new ArgumentOutOfRangeException("index");

                return str[offset + i];
            }
        }

        public StringSegment Slice(int offset, int length)
        {
            if (offset + length > this.length)
                throw new ArgumentOutOfRangeException("length");

            return new StringSegment(str, this.offset + offset, length);
        }

        public StringSegment Slice(int offset) => Slice(offset, length - offset);

        public void SplitAt(int index, out StringSegment left, out StringSegment right)
        {
            if (index < 0 || index > length)
                throw new ArgumentOutOfRangeException(nameof(index));

            left = new StringSegment(str, offset, index);
            right = new StringSegment(str, offset + index, length - index);
        }

        public void SplitAroundFirst(out StringSegment left, out StringSegment right, params string[] substrings)
        {
            if (substrings.Length == 0)
                throw new ArgumentException("Must provide at least one substring to split around");

            int index = int.MaxValue;
            int substringLength = -1;

            foreach(var s in substrings)
            {
                var candidateIndex = IndexOf(s);

                if (candidateIndex < 0)
                    continue;

                if(candidateIndex < index)
                {
                    index = candidateIndex;
                    substringLength = s.Length;
                }
            }

            if(substringLength < 0)
            {
                left = this;
                right = new StringSegment(); // empty
            }
            else
            {
                left = new StringSegment(str, offset, index);
                right = new StringSegment(str, offset + index + substringLength, length - index - substringLength);
            }
        }

        public void SplitAround(string substring, out StringSegment left, out StringSegment right)
        {
            var index = IndexOf(substring);

            if (index < 0)
            {
                left = this;
                right = new StringSegment(); // empty
            }
            else
            {
                left = new StringSegment(str, offset, index);
                right = new StringSegment(str, offset + index + substring.Length, length - index - substring.Length);
            }
        }

        public StringSegment Trim()
        {
            if (IsEmpty)
                return this;

            var startIndex = 0;
            var trimmedLength = length;

            while (char.IsWhiteSpace(this[startIndex]))
                startIndex++;

            while (trimmedLength > 0 && char.IsWhiteSpace(this[trimmedLength - 1]))
                trimmedLength--;

            trimmedLength -= startIndex;

            if (trimmedLength < 0)
                return Empty;

            return Slice(startIndex, trimmedLength);
        }

        public int IndexOf(string substring)
        {
            if (IsEmpty)
                return -1;

            var index = str.IndexOf(substring, offset, length);

            if (index < 0)
                return index;

            return index - offset;
        }

        public int LastIndexOf(string substring)
        {
            if (IsEmpty)
                return -1;

            var index = str.LastIndexOf(substring, offset + length - 1, length);

            if (index < 0)
                return index;

            return index - offset;
        }

        public static StringSegment Empty => new StringSegment();

        public ReadOnlySpan<char> AsSpan() => str.AsSpan(offset, length);

        public bool Equals(in StringSegment other, StringComparison comparison) =>
            AsSpan().Equals(other.AsSpan(), comparison);

        public bool Equals(string other, StringComparison comparison) =>
            AsSpan().Equals(other.AsSpan(), comparison);

        public override int GetHashCode() => string.GetHashCode(AsSpan());

        public override string ToString()
        {
            // When it's empty, always return empty string, rather than null. This way we do not
            // leak the state of the internal string, where sometimes it could be empty, sometimes null
            if (length == 0)
                return string.Empty;

            // Optimization in case it wasn't sliced at all
            if (offset == 0 && length == str.Length)
                return str;

            return AsSpan().ToString();
        }
    }
}
