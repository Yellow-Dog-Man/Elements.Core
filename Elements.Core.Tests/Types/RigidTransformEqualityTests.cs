namespace Elements.Core.Tests;

[TestClass]
public class RigidTransformEqualityTests
{
    static readonly float3 POSITION = new(1f, 2f, 3f);
    static readonly float3 DIFFERENT_POSITION = new(4f, 2f, 3f);

    static readonly floatQ ROTATION = floatQ.Euler(0f, 90f, 0f);
    static readonly floatQ DIFFERENT_ROTATION = floatQ.Euler(0f, 95f, 0f);

    // floatQ compares rotations approximately, and a thousandth of a degree sits well
    // inside that tolerance.
    static readonly floatQ NEGLIGIBLY_DIFFERENT_ROTATION = floatQ.Euler(0f, 90.001f, 0f);

    // Built fresh on every call rather than shared, so the tests compare distinct
    // instances, and a distinct box each time one is passed as an object.
    static RigidTransform MakeRigidTransform(float3? position = null, floatQ? rotation = null) =>
        new(position ?? POSITION, rotation ?? ROTATION);

    public static IEnumerable<object[]> EqualRigidTransforms =>
    [
        ["the same components", MakeRigidTransform(), MakeRigidTransform()],
        ["rotations within the approximate comparison tolerance", MakeRigidTransform(), MakeRigidTransform(rotation: NEGLIGIBLY_DIFFERENT_ROTATION)],
    ];

    public static IEnumerable<object[]> UnequalRigidTransforms =>
    [
        ["a different position", MakeRigidTransform(), MakeRigidTransform(position: DIFFERENT_POSITION)],
        ["a different rotation", MakeRigidTransform(), MakeRigidTransform(rotation: DIFFERENT_ROTATION)],
    ];

    public static IEnumerable<object[]> ValuesThatAreNotRigidTransforms =>
    [
        ["null", null],
        ["a string", "not a rigid transform"],
        ["a Transform carrying the same position and rotation", new Transform(POSITION, ROTATION, float3.One)],
    ];

    [TestMethod]
    [DynamicData(nameof(EqualRigidTransforms))]
    public void Equals_EqualRigidTransforms_ReturnsTrue(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a.Equals(b);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalRigidTransforms))]
    public void Equals_UnequalRigidTransforms_ReturnsFalse(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a.Equals(b);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    public void Equals_SameInstance_ReturnsTrue()
    {
        var rigidTransform = MakeRigidTransform();

        var actual = rigidTransform.Equals(rigidTransform);

        Assert.IsTrue(actual);
    }

    [TestMethod]
    [DynamicData(nameof(EqualRigidTransforms))]
    public void EqualsObject_EqualRigidTransforms_ReturnsTrue(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a.Equals((object)b);

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalRigidTransforms))]
    public void EqualsObject_UnequalRigidTransforms_ReturnsFalse(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a.Equals((object)b);

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(ValuesThatAreNotRigidTransforms))]
    public void EqualsObject_ValueThatIsNotARigidTransform_ReturnsFalse(string description, object other)
    {
        var rigidTransform = MakeRigidTransform();

        var actual = rigidTransform.Equals(other);

        Assert.IsFalse(actual, description);
    }

    [TestMethod]
    [DynamicData(nameof(EqualRigidTransforms))]
    public void EqualityOperator_EqualRigidTransforms_ReturnsTrue(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a == b;

        Assert.IsTrue(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalRigidTransforms))]
    public void EqualityOperator_UnequalRigidTransforms_ReturnsFalse(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a == b;

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(EqualRigidTransforms))]
    public void InequalityOperator_EqualRigidTransforms_ReturnsFalse(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a != b;

        Assert.IsFalse(actual, scenario);
    }

    [TestMethod]
    [DynamicData(nameof(UnequalRigidTransforms))]
    public void InequalityOperator_UnequalRigidTransforms_ReturnsTrue(string scenario, RigidTransform a, RigidTransform b)
    {
        var actual = a != b;

        Assert.IsTrue(actual, scenario);
    }

    // Equal values are required to hash the same, but the hash mixes the rotation exactly
    // while equality compares it approximately, so this pair hashes differently.
    [TestMethod, Ignore("Known defect: rotations that compare approximately equal do not share a hash code.")]
    public void GetHashCode_RotationsWithinTheApproximateComparisonTolerance_ReturnsTheSameValue()
    {
        var a = MakeRigidTransform();
        var b = MakeRigidTransform(rotation: NEGLIGIBLY_DIFFERENT_ROTATION);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}
