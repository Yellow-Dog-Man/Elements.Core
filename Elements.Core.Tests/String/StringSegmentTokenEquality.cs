using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Elements.Core.Tests;

[TestClass]
public class StringSegmentTokenEqualityTests
{
    // Built at runtime from chars, so it is not interned alongside the literals below,
    // which forces Equals down the substring comparison path instead of the
    // ReferenceEquals(WholeString, ...) fast path.
    static string Uninterned(string value) => new string(value.ToCharArray());

    [TestMethod]
    public void TestSameBackingStringAndRangeIsEqual()
    {
        const string whole = "xxhelloxx";

        var a = new StringSegmentToken(whole, 2, 5);
        var b = new StringSegmentToken(whole, 2, 5);

        Assert.AreEqual("hello", a.Substring, "Sanity check on the segment");
        Assert.IsTrue(a.Equals(b), "Equals(StringSegmentToken)");
        Assert.IsTrue(a.Equals((object)b), "Equals(object)");
        Assert.IsTrue(a == b, "operator ==");
        Assert.IsFalse(a != b, "operator !=");
    }

    [TestMethod]
    public void TestSameContentAcrossDifferentBackingStringsIsEqual()
    {
        var a = new StringSegmentToken("xxhelloxx", 2, 5);
        var b = new StringSegmentToken(Uninterned("hello"));

        Assert.IsTrue(a.Equals(b), "Equals(StringSegmentToken)");
        Assert.IsTrue(a == b, "operator ==");
    }

    [TestMethod]
    public void TestEqualValuesShareHashCode()
    {
        // Equal tokens must hash the same regardless of which string backs them,
        // otherwise they cannot be used as keys.
        var a = new StringSegmentToken("xxhelloxx", 2, 5);
        var b = new StringSegmentToken(Uninterned("hello"));

        Assert.IsTrue(a.Equals(b), "Precondition: the tokens are equal");
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), "Equal tokens, different backing strings");
    }

    [TestMethod]
    public void TestDifferentContentIsNotEqual()
    {
        var a = new StringSegmentToken("hello");
        var b = new StringSegmentToken("world");

        Assert.IsFalse(a.Equals(b), "Equals(StringSegmentToken)");
        Assert.IsFalse(a == b, "operator ==");
        Assert.IsTrue(a != b, "operator !=");
    }

    [TestMethod]
    public void TestDifferentLengthIsNotEqual()
    {
        const string whole = "hello";

        var a = new StringSegmentToken(whole, 0, 5);
        var b = new StringSegmentToken(whole, 0, 4);

        Assert.IsFalse(a.Equals(b), "Equals(StringSegmentToken)");
        Assert.IsFalse(a == b, "operator ==");
    }

    [TestMethod]
    public void TestEqualityIsReflexiveAndSymmetric()
    {
        var a = new StringSegmentToken("xxhelloxx", 2, 5);
        var b = new StringSegmentToken(Uninterned("hello"));

        Assert.IsTrue(a.Equals(a), "Reflexive");
        Assert.AreEqual(a.Equals(b), b.Equals(a), "Symmetric");
    }

    [TestMethod]
    public void TestNullHandling()
    {
        var a = new StringSegmentToken("hello");
        StringSegmentToken none = null;

        // ReSharper disable once ExpressionIsAlwaysNull
        Assert.IsFalse(a.Equals(none), "Equals(StringSegmentToken) with null");
        Assert.IsFalse(a.Equals((object)null), "Equals(object) with null");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        Assert.IsFalse(a == none, "operator == with null on the right");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        Assert.IsFalse(none == a, "operator == with null on the left");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        Assert.IsTrue(a != none, "operator != with null");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        Assert.IsTrue(none == null, "Two nulls are equal");
    }

    [TestMethod]
    public void TestEqualsObjectRejectsForeignTypes()
    {
        var a = new StringSegmentToken("hello");

        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.IsFalse(a.Equals("hello"), "Equal content, but a raw string");
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.IsFalse(a.Equals(42), "Unrelated type");
    }

    [TestMethod]
    public void TestUsableAsDictionaryKey()
    {
        var map = new Dictionary<StringSegmentToken, string>
        {
            [new StringSegmentToken("xxhelloxx", 2, 5)] = "value"
        };

        var lookup = new StringSegmentToken(Uninterned("hello"));

        Assert.IsTrue(map.TryGetValue(lookup, out var found), "Lookup by an equal key");
        Assert.AreEqual("value", found, "Retrieved value");

        map[lookup] = "replaced";
        Assert.HasCount(1, map, "Equal keys must not create a second entry");
    }

    [TestMethod]
    public void TestUsableInHashSet()
    {
        var set = new HashSet<StringSegmentToken>();

        Assert.IsTrue(set.Add(new StringSegmentToken("xxhelloxx", 2, 5)), "First add");
        Assert.IsFalse(set.Add(new StringSegmentToken(Uninterned("hello"))), "Duplicate add");
    }
}
