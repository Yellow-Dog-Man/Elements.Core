namespace Elements.Core.Tests
{
    [TestClass]
    public class CoderNullable
    {
        [TestMethod]
        public void TestEquals()
        {
            Assert.IsTrue(Coder<int?>.Equals(null, null), conditionExpression: "null == null");
            Assert.IsFalse(Coder<int?>.Equals(1, null), conditionExpression: "1 != null");
            Assert.IsFalse(Coder<int?>.Equals(null, 1), conditionExpression: "null != 1");
            Assert.IsTrue(Coder<int?>.Equals(1, 1), conditionExpression: "1 == 1");
            Assert.IsFalse(Coder<int?>.Equals(1, 2), conditionExpression: "1 != 2");
        }
    }
}
