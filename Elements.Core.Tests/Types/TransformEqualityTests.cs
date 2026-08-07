using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests;

[TestClass]
public class TransformEqualityTests
{
    // Built fresh each time rather than shared, so the tests compare distinct
    // instances and different boxed value handed back twice.
    static Transform Make(float positionOffset = 0f) => new(
        new float3(1f + positionOffset, 2f, 3f),
        floatQ.Euler(0f, 90f, 0f),
        new float3(0.5f, 0.25f, 0.125f));

    [TestMethod]
    public void TestEqualValuesAreEqual()
    {
        var a = Make();
        var b = Make();

        Assert.IsTrue(a.Equals(b), "Equals(Transform)");
        Assert.IsTrue(a.Equals((object)b), "Equals(object)");
        Assert.IsTrue(a == b, "operator ==");
        Assert.IsFalse(a != b, "operator !=");
    }

    [TestMethod]
    public void TestApproximatelyEqualRotationsAreEqual()
    {
        var a = new Transform(float3.Zero, floatQ.Euler(0f, 0f, 0f), float3.One);
        var b = new Transform(float3.Zero, floatQ.Euler(0f, 0.001f, 0f), float3.One);

        Assert.IsTrue(a == b, "Precondition: the rotations compare approximately equal");
    }

    [TestMethod]
    public void TestEqualsIncludesPosition()
    {
        var a = new Transform(float3.Forward, floatQ.Euler(0f, 0f, 0f), float3.One);
        var b = new Transform(float3.Zero, floatQ.Euler(0f, 0f, 0f), float3.One);

        Assert.AreNotEqual(a, b, "Transforms with different positions were not counted as equal.");
    }

    [TestMethod]
    public void TestEqualsIncludesRotation()
    {
        var a = new Transform(float3.Zero, floatQ.Euler(0f, 0f, 0f), float3.One);
        var b = new Transform(float3.Zero, floatQ.Euler(0f, 5f, 0f), float3.One);

        Assert.AreNotEqual(a, b, "Transforms with different rotations were not counted as equal.");
    }

    [TestMethod]
    public void TestEqualsIncludesScale()
    {
        var a = new Transform(float3.Zero, floatQ.Euler(0f, 0f, 0f), float3.One);
        var b = new Transform(float3.Zero, floatQ.Euler(0f, 0f, 0f), float3.Forward);

        Assert.AreNotEqual(a, b, "Transforms with different scales were not counted as equal.");
    }

    [TestMethod]
    public void TestDifferingPositionIsNotEqual()
    {
        var a = Make();
        var b = Make(1f);

        Assert.IsFalse(a.Equals(b), "Equals(Transform)");
        Assert.IsFalse(a.Equals((object)b), "Equals(object)");
        Assert.IsFalse(a == b, "operator ==");
        Assert.IsTrue(a != b, "operator !=");
    }

    [TestMethod]
    public void TestDifferingRotationIsNotEqual()
    {
        var a = new Transform(float3.Zero, floatQ.Identity, float3.One);
        var b = new Transform(float3.Zero, floatQ.Euler(0f, 90f, 0f), float3.One);

        Assert.IsFalse(a == b, "operator ==");
        Assert.IsTrue(a != b, "operator !=");
    }

    [TestMethod]
    public void TestDifferingScaleIsNotEqual()
    {
        var a = new Transform(float3.Zero, floatQ.Identity, float3.One);
        var b = new Transform(float3.Zero, floatQ.Identity, float3.Zero);

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
        Assert.IsFalse(a.Equals("not a transform"), "String");
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.IsFalse(a.Equals(new RigidTransform(a.position, a.rotation)), "RigidTransform");
    }
}
