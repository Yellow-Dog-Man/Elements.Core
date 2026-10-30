using System;
using System.Text;

namespace Elements.Core
{
    public class StringSegmentToken : IEquatable<StringSegmentToken>
    {
        public readonly string WholeString;

        public readonly int StartIndex;
        public readonly int Length;

        public int EndIndex { get { return StartIndex + Length; } }

        string _cachedSubstring;

        // Store the entire string
        public StringSegmentToken(string str)
        {
            _cachedSubstring = str;
            WholeString = str;
            StartIndex = 0;
            Length = str.Length;
        }

        public StringSegmentToken(string original, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > original.Length)
                throw new ArgumentOutOfRangeException();

            // Automatically compute length from the startindex to the end of the original
            if (length == -1)
                length = original.Length - startIndex;

            if (length < 0 || (startIndex + length) > original.Length)
                throw new ArgumentOutOfRangeException();

            WholeString = original;
            StartIndex = startIndex;
            Length = length;
        }

        public string Substring
        {
            get
            {
                if (_cachedSubstring == null)
                    _cachedSubstring = WholeString.Substring(StartIndex, Length);

                return _cachedSubstring;
            }
        }

        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");

                return WholeString[StartIndex + index];
            }
        }

        public ReadOnlySpan<char> AsSpan() => WholeString.AsSpan(StartIndex, Length);

        public StringSegmentToken SubSegment(int startIndex)
        {
            return SubSegment(startIndex, Length - startIndex);
        }

        public StringSegmentToken SubSegment(int startIndex, int length)
        {
            if (startIndex < 0 || (startIndex + length) > Length)
                throw new ArgumentOutOfRangeException();

            // Automatically compute length from the startindex to the end of the original
            // It needs to be calculated at this point, even though building new string segment
            // has the same functionality - that works on the whole original string though and not
            // just the subsegment this represents
            if (length == -1)
                length = Length - startIndex;

            if (startIndex == 0 && length == Length)
                return this; // just wants the whole thing

            return new StringSegmentToken(WholeString,
                StartIndex + startIndex, length);
        }

        public int IndexOf(string value)
        {
            var index = WholeString.IndexOf(value, StartIndex, Length);
            if (index < 0)
                return index;
            return index - StartIndex;
        }

        public int IndexOf(char value)
        {
            var index = WholeString.IndexOf(value, StartIndex, Length);
            if (index < 0)
                return index;
            return index - StartIndex;
        }

        public override string ToString()
        {
            return Substring;
        }

        public bool Equals(StringSegmentToken other)
        {
            if (other == null)
                return false;

            if (object.ReferenceEquals(WholeString, other.WholeString) &&
                StartIndex == other.StartIndex && Length == other.Length)
                return true;

            if (Length != other.Length)
                return false;

            // length equals, but they are either different whole strings or different parts
            // perform a comparison of the actual substring
            return string.CompareOrdinal(this.WholeString, this.StartIndex, other.WholeString, other.StartIndex, Length)
                   == 0;
        }

        public override bool Equals(object obj)
        {
            var other = obj as StringSegmentToken;

            if (other == null)
                return false;

            return this.Equals(other);
        }

        public static bool operator ==(StringSegmentToken a, StringSegmentToken b)
        {
            if (ReferenceEquals(a, null))
                return ReferenceEquals(b, null);

            return a.Equals(b);
        }

        public static bool operator !=(StringSegmentToken a, StringSegmentToken b)
        {
            return !(a == b);
        }

        public override int GetHashCode() => string.GetHashCode(AsSpan());
    }

    public static class StringSegmentTokenExtensions
    {
        public static void Append(this StringBuilder builder, StringSegmentToken segment)
        {
            builder.Append(segment.WholeString, segment.StartIndex, segment.Length);
        }

        public static void AppendLine(this StringBuilder builder, StringSegmentToken segment)
        {
            builder.Append(segment);
            builder.AppendLine();
        }
    }
}