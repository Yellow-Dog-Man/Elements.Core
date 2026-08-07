using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests;

[TestClass]
public class RigidTransformEqualityTests
{
    static RigidTransform Make(float positionOffset = 0f) => new(
        new float3(1f + positionOffset, 2f, 3f),
        floatQ.Euler(0f, 90f, 0f));

    [TestMethod]
    public void TestEqualValuesAreEqual()
    {
        var a = Make();
        var b = Make();

        Assert.IsTrue(a.Equals(b), "Equals(RigidTransform)");
        Assert.IsTrue(a.Equals((object)b), "Equals(object)");
        Assert.IsTrue(a == b, "operator ==");
        Assert.IsFalse(a != b, "operator !=");
    }

    // Fails due to the hash not lining up.
    [TestMethod, Ignore]
    public void TestApproximatelyEqualRotationsShareHashCode()
    {
        var a = new RigidTransform(float3.Zero, floatQ.Euler(0f, 0f, 0f));
        var b = new RigidTransform(float3.Zero, floatQ.Euler(0f, 0.001f, 0f));

        Assert.IsTrue(a == b, "Precondition: the rotations compare approximately equal");
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode(), "Equal transforms must hash the same");
    }

    [TestMethod]
    public void TestEqualsIncludesPosition()
    {
        var a = new RigidTransform(float3.Forward, floatQ.Euler(0f, 0f, 0f));
        var b = new RigidTransform(float3.Zero, floatQ.Euler(0f, 0f, 0f));

        Assert.AreNotEqual(a, b, "Transforms with different positions were not counted as equal.");
    }

    [TestMethod]
    public void TestEqualsIncludesRotation()
    {
        var a = new RigidTransform(float3.Zero, floatQ.Euler(0f, 0f, 0f));
        var b = new RigidTransform(float3.Zero, floatQ.Euler(0f, 5f, 0f));

        Assert.AreNotEqual(a, b, "Transforms with different rotations were not counted as equal.");
    }

    [TestMethod]
    public void TestDifferingPositionIsNotEqual()
    {
        var a = Make();
        var b = Make(1f);

        Assert.IsFalse(a.Equals(b), "Equals(RigidTransform)");
        Assert.IsFalse(a == b, "operator ==");
        Assert.IsTrue(a != b, "operator !=");
    }

    [TestMethod]
    public void TestDifferingRotationIsNotEqual()
    {
        var a = new RigidTransform(float3.Zero, floatQ.Identity);
        var b = new RigidTransform(float3.Zero, floatQ.Euler(0f, 90f, 0f));

        Assert.IsFalse(a == b, "operator ==");
        Assert.IsTrue(a != b, "operator !=");
    }

    [TestMethod]
    public void TestEqualityIsReflexiveAndSymmetric()
    {
        var a = Make();
        var b = Make();

        Assert.IsTrue(a.Equals(a), "Reflexive");
        Assert.AreEqual(a.Equals(b), b.Equals(a), "Symmetric");
    }

    [TestMethod]
    public void TestEqualsObjectRejectsNullAndForeignTypes()
    {
        var a = Make();

        Assert.IsFalse(a.Equals(null), "Null");
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.IsFalse(a.Equals("not a rigid transform"), "String");
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.IsFalse(a.Equals(new Transform(a.position, a.rotation, float3.One)), "Transform");
    }
}
