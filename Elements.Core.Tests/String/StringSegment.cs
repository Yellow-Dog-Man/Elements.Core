namespace Elements.Core.Tests
{
    [TestClass]
    public class StringSegmentTests
    {
        [TestMethod]
        public void TestTrim()
        {
            const string INNER_TEXT = "Test And Blap";

            var segment = new StringSegment(INNER_TEXT);
            var trimmed = segment.Trim();

            Assert.AreEqual(INNER_TEXT, trimmed.ToString(), "Nothing to trim");

            segment = new StringSegment($"   {INNER_TEXT} ");
            trimmed = segment.Trim();

            Assert.AreEqual(INNER_TEXT, trimmed.ToString(), "Spaces to trim");
        }

        [DataRow(0, "", "abcdefg")]
        [DataRow(1, "a", "bcdefg")]
        [DataRow(2, "ab", "cdefg")]
        [DataRow(3, "abc", "defg")]
        [DataRow(4, "abcd", "efg")]
        [DataRow(5, "abcde", "fg")]
        [DataRow(6, "abcdef", "g")]
        [DataRow(7, "abcdefg", "")]
        [TestMethod]
        public void TestSplit(int index, string leftExpected, string rightExpected)
        {
            var segment = new StringSegment(" abcdefg ");

            // We trim it, just to make sure the index operations work properly with offsets and trimmed lengths
            segment = segment.Trim();

            segment.SplitAt(index, out var left, out var right);

            Assert.AreEqual(leftExpected, left.ToString(), "Left split");
            Assert.AreEqual(rightExpected, right.ToString(), "Right split");
        }

        [DataRow("+")]
        [DataRow("AND")]
        [DataRow(" --> ")]
        [TestMethod]
        public void TestSplitAround(string separator)
        {
            const string A = "Potatoes";
            const string B = "Fruits and vegetables";
            const string C = "Mayo";

            var segment = new StringSegment($"   {A}{separator}{B}{separator}{C}  ");
            segment = segment.Trim();

            segment.SplitAround(separator, out var a, out var bcd);
            bcd.SplitAround(separator, out var b, out var cd);
            cd.SplitAround(separator, out var c, out var d);

            Assert.AreEqual(A, a.ToString());
            Assert.AreEqual(B, b.ToString());
            Assert.AreEqual(C, c.ToString());
            Assert.AreEqual("", d.ToString());
        }

        [TestMethod]
        public void TestLastIndexOf()
        {
            const string FULL_STRING = "[[[[]]]]";

            var segment = new StringSegment(FULL_STRING);

            Assert.AreEqual(FULL_STRING.LastIndexOf("]"), segment.LastIndexOf("]"), "Original full string");

            segment = segment.Slice(1, segment.length - 2);

            var substring = FULL_STRING.Substring(1, FULL_STRING.Length - 2);

            Assert.AreEqual(substring.LastIndexOf("]"), segment.LastIndexOf("]"), "Substring");
        }

        [TestMethod]
        public void TestLastIndexOfWord()
        {
            const string SUBSTRING = "Mlem";
            const string FULL_STRING = "Potatoes and " + SUBSTRING + " with sauce " + SUBSTRING + ", and also " + SUBSTRING;

            var segment = new StringSegment($"   {FULL_STRING}   ");
            segment = segment.Trim();

            var expected = FULL_STRING.LastIndexOf(SUBSTRING);
            var actual = segment.LastIndexOf(SUBSTRING);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestHashCodeIsContentBased()
        {
            // The same text reached through two different backing strings has to hash
            // identically, otherwise segments cannot be used as keys.
            var whole = new StringSegment("hello");
            var sliced = new StringSegment("xxhelloxx", 2, 5);

            Assert.AreEqual("hello", sliced.ToString(), "Sanity check on the segment");
            Assert.AreEqual(whole.GetHashCode(), sliced.GetHashCode(), "Same content, different backing strings");
        }

        [TestMethod]
        public void TestHashCodeMatchesEquivalentString()
        {
            // Segments hash ordinally, the same way the equivalent string does, so the
            // two can share a hash based lookup.
            const string text = "Potatoes";

            var segment = new StringSegment($"  {text}  ").Trim();

            Assert.AreEqual(text.GetHashCode(), segment.GetHashCode(), "Segment against the equivalent string");
        }

        [TestMethod]
        public void TestEmptySegmentsShareHashCode()
        {
            var empty = StringSegment.Empty;
            var emptyString = new StringSegment("");
            var slicedEmpty = new StringSegment("abc", 1, 0);

            Assert.AreEqual(empty.GetHashCode(), emptyString.GetHashCode(), "Default against an empty string");
            Assert.AreEqual(empty.GetHashCode(), slicedEmpty.GetHashCode(), "Default against a zero length slice");
        }

        [TestMethod]
        public void TestDifferentContentHashesDiffer()
        {
            var a = new StringSegment("Potatoes");
            var b = new StringSegment("Mayo");

            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode(), "Distinct content should not collide");
        }
    }
}
