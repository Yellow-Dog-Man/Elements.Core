namespace Elements.Core.Tests;

[TestClass]
public class TransformEqualityTests
{
    static readonly float3 POSITION = new(1f, 2f, 3f);
    static readonly float3 DIFFERENT_POSITION = new(4f, 2f, 3f);

    static readonly floatQ ROTATION = floatQ.Euler(0f, 90f, 0f);
    static readonly floatQ DIFFERENT_ROTATION = floatQ.Euler(0f, 95f, 0f);

    // floatQ compares rotations approximately, and a thousandth of a degree sits well
    // inside that tolerance.
    static readonly floatQ NEGLIGIBLY_DIFFERENT_ROTATION = floatQ.Euler(0f, 90.001f, 0f);

    static readonly float3 SCALE = new(0.5f, 0.25f, 0.125f);
    static readonly float3 DIFFERENT_SCALE = float3.One;

    // Built fresh on every call rather than shared, so the tests compare distinct
    // instances, and a distinct box each time one is passed as an object.
    static Transform MakeTransform(float3? position = null, floatQ? rotation = null, float3? scale = null) =>
        new(position ?? POSITION, rotation ?? ROTATION, scale ?? SCALE);

    public static IEnumerable<object[]> EqualTransforms =>
    [
        ["the same components", MakeTransform(), MakeTransform()],
        ["rotations within the approximate comparison tolerance", MakeTransform(), MakeTransform(rotation: NEGLIGIBLY_DIFFERENT_ROTATION)],
    ];

    public static IEnumerable<object[]> UnequalTransforms =>
    [
        ["a different position", MakeTransform(), MakeTransform(position: DIFFERENT_POSITION)],
        ["a different rotation", MakeTransform(), MakeTransform(rotation: DIFFERENT_ROTATION)],
        ["a different scale", MakeTransform(), MakeTransform(scale: DIFFERENT_SCALE)],
    ];

    public static IEnumerable<object[]> ValuesThatAreNotTransforms =>
    [
        ["null", null],
        ["a string", "not a transform"],
        ["a RigidTransform carrying the same position and rotation", new RigidTransform(POSITION, ROTATION)],
    ];

    [TestMethod]
    [DynamicData(nameof(EqualTransforms))]
    public void Equals_EqualTransforms_ReturnsTrue(string scenario, Transform a, Transform b)
    {
        var actual = a.Equals(b);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTransforms))]
    public void Equals_UnequalTransforms_ReturnsFalse(string scenario, Transform a, Transform b)
    {
        var actual = a.Equals(b);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var transform = MakeTransform();

        var actual = transform.Equals(transform);

        Assert.IsTrue(actual);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTransforms))]
    public void EqualsObject_EqualTransforms_ReturnsTrue(string scenario, Transform a, Transform b)
    {
        var actual = a.Equals((object)b);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTransforms))]
    public void EqualsObject_UnequalTransforms_ReturnsFalse(string scenario, Transform a, Transform b)
    {
        var actual = a.Equals((object)b);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(ValuesThatAreNotTransforms))]
    public void EqualsObject_ValueThatIsNotATransform_ReturnsFalse(string description, object other)
    {
        var transform = MakeTransform();

        var actual = transform.Equals(other);

        Assert.IsFalse(actual, description);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTransforms))]
    public void EqualityOperator_EqualTransforms_ReturnsTrue(string scenario, Transform a, Transform b)
    {
        var actual = a == b;

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTransforms))]
    public void EqualityOperator_UnequalTransforms_ReturnsFalse(string scenario, Transform a, Transform b)
    {
        var actual = a == b;

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(EqualTransforms))]
    public void InequalityOperator_EqualTransforms_ReturnsFalse(string scenario, Transform a, Transform b)
    {
        var actual = a != b;

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalTransforms))]
    public void InequalityOperator_UnequalTransforms_ReturnsTrue(string scenario, Transform a, Transform b)
    {
        var actual = a != b;

        Assert.IsTrue(actual, scenario);
    }
}
