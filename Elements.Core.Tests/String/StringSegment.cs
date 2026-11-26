namespace Elements.Core.Tests
{
    [TestClass]
    public class StringSegmentTests
    {
        const string POTATOES = "Potatoes";

        // Four characters long, so it doubles as the offset of POTATOES when the two are
        // concatenated.
        const string MAYO = "Mayo";

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

        // Segments hash ordinally, the same way the equivalent string does, so the two can
        // share a hash based lookup.
        [DataRow(POTATOES, POTATOES, 0, DisplayName = "Whole string")]
        [DataRow(POTATOES, "  " + POTATOES + "  ", 2, DisplayName = "Slice out of a padded string")]
        [DataRow("", "", 0, DisplayName = "Empty string")]
        [DataRow("", POTATOES, 1, DisplayName = "Zero length slice")]
        [TestMethod]
        public void GetHashCode_Segment_MatchesTheEquivalentString(string expectedContent, string backingString, int offset)
        {
            var segment = new StringSegment(backingString, offset, expectedContent.Length);

            var actual = segment.GetHashCode();

            Assert.AreEqual(expectedContent.GetHashCode(), actual);
        }

        [TestMethod]
        public void GetHashCode_DefaultSegment_MatchesTheEmptyString()
        {
            var segment = StringSegment.Empty;

            var actual = segment.GetHashCode();

            Assert.AreEqual(string.Empty.GetHashCode(), actual);
        }

        // The same text reached through two different backing strings has to hash
        // identically, otherwise segments cannot be used as keys.
        [DataRow(POTATOES, "  " + POTATOES + "  ", 2, DisplayName = "Slice out of a padded string")]
        [DataRow(POTATOES, POTATOES + MAYO, 0, DisplayName = "Slice off the start of a longer string")]
        [DataRow(POTATOES, MAYO + POTATOES, 4, DisplayName = "Slice off the end of a longer string")]
        [TestMethod]
        public void GetHashCode_SegmentsWithTheSameContent_ReturnsTheSameValue(string content, string backingString, int offset)
        {
            var whole = new StringSegment(content);
            var sliced = new StringSegment(backingString, offset, content.Length);

            Assert.AreEqual(whole.GetHashCode(), sliced.GetHashCode());
        }

        [DataRow(POTATOES, MAYO, DisplayName = "Unrelated content")]
        [DataRow(POTATOES, "potatoes", DisplayName = "Differing only in case")]
        [DataRow(POTATOES, "Potato", DisplayName = "Prefix of the other")]
        [TestMethod]
        public void GetHashCode_SegmentsWithDifferentContent_ReturnsDifferentValues(string left, string right)
        {
            var a = new StringSegment(left);
            var b = new StringSegment(right);

            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}
