namespace Elements.Core.Tests;

[TestClass]
public class StringSegmentTokenEqualityTests
{
    const string HELLO = "hello";
    const string PADDED_HELLO = "xxhelloxx";
    const int HELLO_OFFSET = 2;
    const int HELLO_LENGTH = 5;

    // Same length as HELLO, so the comparison cannot short circuit on the length.
    const string WORLD = "world";

    // A prefix of HELLO, so the comparison differs only in length.
    const int SHORTER_LENGTH = 4;

    const string STORED_VALUE = "value";
    const string REPLACEMENT_VALUE = "replaced";

    // Built at runtime from chars, so it is not interned alongside the literals above,
    // which forces Equals down the substring comparison path instead of the
    // ReferenceEquals(WholeString, ...) fast path.
    static string Uninterned(string value) => new string(value.ToCharArray());

    static StringSegmentToken SlicedHello() => new(PADDED_HELLO, HELLO_OFFSET, HELLO_LENGTH);

    static StringSegmentToken StandaloneHello() => new(Uninterned(HELLO));

    public static IEnumerable<object[]> EqualTokens =>
    [
        ["the same backing string and range", SlicedHello(), SlicedHello()],
        ["the same content out of different backing strings", SlicedHello(), StandaloneHello()],
    ];

    public static IEnumerable<object[]> UnequalTokens =>
    [
        ["different content of the same length", StandaloneHello(), new StringSegmentToken(WORLD)],
        ["the same backing string over different lengths", new StringSegmentToken(HELLO, 0, HELLO_LENGTH), new StringSegmentToken(HELLO, 0, SHORTER_LENGTH)],
    ];

    public static IEnumerable<object[]> ValuesThatAreNotTokens =>
    [
        ["a raw string holding equal content", HELLO],
        ["an unrelated type", 42],
    ];

    [TestMethod]
    [DynamicData(nameof(EqualTokens))]
    public void Equals_EqualTokens_ReturnsTrue(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a.Equals(b);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTokens))]
    public void Equals_UnequalTokens_ReturnsFalse(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a.Equals(b);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTokens))]
    public void Equals_EqualTokensWithArgumentsSwapped_ReturnsTrue(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = b.Equals(a);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTokens))]
    public void Equals_UnequalTokensWithArgumentsSwapped_ReturnsFalse(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = b.Equals(a);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var token = SlicedHello();

        var actual = token.Equals(token);

        Assert.IsTrue(actual);
    }

    [TestMethod]
    public void Equals_Null_ReturnsFalse()
    {
        var token = StandaloneHello();
        StringSegmentToken none = null;

        var actual = token.Equals(none);

        Assert.IsFalse(actual);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTokens))]
    public void EqualsObject_EqualTokens_ReturnsTrue(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a.Equals((object)b);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTokens))]
    public void EqualsObject_UnequalTokens_ReturnsFalse(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a.Equals((object)b);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    public void EqualsObject_Null_ReturnsFalse()
    {
        var token = StandaloneHello();

        var actual = token.Equals((object)null);

        Assert.IsFalse(actual);
    }

    [TestMethod]
    [DynamicData(nameof(ValuesThatAreNotTokens))]
    public void EqualsObject_ValueThatIsNotAToken_ReturnsFalse(string description, object other)
    {
        var token = StandaloneHello();

        var actual = token.Equals(other);

        Assert.IsFalse(actual, description);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTokens))]
    public void EqualityOperator_EqualTokens_ReturnsTrue(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a == b;

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTokens))]
    public void EqualityOperator_UnequalTokens_ReturnsFalse(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a == b;

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    public void EqualityOperator_NullOnTheRight_ReturnsFalse()
    {
        var token = StandaloneHello();
        StringSegmentToken none = null;

        var actual = token == none;

        Assert.IsFalse(actual);
    }

    [TestMethod]
    public void EqualityOperator_NullOnTheLeft_ReturnsFalse()
    {
        var token = StandaloneHello();
        StringSegmentToken none = null;

        var actual = none == token;

        Assert.IsFalse(actual);
    }

    [TestMethod]
    public void EqualityOperator_NullOnBothSides_ReturnsTrue()
    {
        StringSegmentToken none = null;

        var actual = none == null;

        Assert.IsTrue(actual);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTokens))]
    public void InequalityOperator_EqualTokens_ReturnsFalse(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a != b;

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTokens))]
    public void InequalityOperator_UnequalTokens_ReturnsTrue(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a != b;

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    public void InequalityOperator_Null_ReturnsTrue()
    {
        var token = StandaloneHello();
        StringSegmentToken none = null;

        var actual = token != none;

        Assert.IsTrue(actual);
    }

    // Equal tokens have to hash the same regardless of which string backs them,
    // otherwise they cannot be used as keys.
    [TestMethod]
    [DynamicData(nameof(EqualTokens))]
    public void GetHashCode_EqualTokens_ReturnsTheSameValue(string scenario, StringSegmentToken a, StringSegmentToken b)
    {
        var actual = a.GetHashCode();

        Assert.AreEqual(b.GetHashCode(), actual, scenario);
    }

    [TestMethod]
    public void AsDictionaryKey_EqualToken_FindsTheStoredValue()
    {
        var map = new Dictionary<StringSegmentToken, string> { [SlicedHello()] = STORED_VALUE };

        var actual = map[StandaloneHello()];

        Assert.AreEqual(STORED_VALUE, actual);
    }

    [TestMethod]
    public void AsDictionaryKey_EqualToken_ReplacesTheExistingEntry()
    {
        var map = new Dictionary<StringSegmentToken, string> { [SlicedHello()] = STORED_VALUE };

        map[StandaloneHello()] = REPLACEMENT_VALUE;

        Assert.HasCount(1, map);
    }

    [TestMethod]
    public void AsHashSetItem_UnequalToken_IsAdded()
    {
        var set = new HashSet<StringSegmentToken> { SlicedHello() };

        var actual = set.Add(new StringSegmentToken(WORLD));

        Assert.IsTrue(actual);
    }

    [TestMethod]
    public void AsHashSetItem_EqualToken_IsRejectedAsADuplicate()
    {
        var set = new HashSet<StringSegmentToken> { SlicedHello() };

        var actual = set.Add(StandaloneHello());

        Assert.IsFalse(actual);
    }
}
