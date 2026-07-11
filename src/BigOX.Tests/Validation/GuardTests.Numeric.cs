using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
// ReSharper disable once InconsistentNaming
public class GuardTests_Numeric
{
    [TestMethod]
    public void NonZero_Throws_OnZero()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.NonZero(0));
        TestUtils.Expect<ArgumentException>(() => Guard.NonZero(0m));
        TestUtils.Expect<ArgumentException>(() => Guard.NonZero(0.0));
    }

    [TestMethod]
    public void NonZero_Returns_OnNonZero()
    {
        Assert.AreEqual(1, Guard.NonZero(1));
        Assert.AreEqual(-1, Guard.NonZero(-1));
        Assert.AreEqual(1.5, Guard.NonZero(1.5));
    }

    [TestMethod]
    public void Positive_Throws_OnNonPositive()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.Positive(0));
        TestUtils.Expect<ArgumentException>(() => Guard.Positive(-1));
    }

    [TestMethod]
    public void Positive_Returns_OnPositive()
    {
        Assert.AreEqual(10, Guard.Positive(10));
        Assert.AreEqual(10m, Guard.Positive(10m));
    }

    [TestMethod]
    public void NonNegative_Throws_OnNegative()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.NonNegative(-1));
        TestUtils.Expect<ArgumentException>(() => Guard.NonNegative(-1m));
    }

    [TestMethod]
    public void NonNegative_Returns_OnZeroOrPositive()
    {
        Assert.AreEqual(0, Guard.NonNegative(0));
        Assert.AreEqual(5, Guard.NonNegative(5));
        Assert.AreEqual(5m, Guard.NonNegative(5m));
    }

    [TestMethod]
    public void Positive_And_NonNegative_Throw_ArgumentOutOfRangeException()
    {
        // Aligned with the sibling range guards (Minimum/Maximum/WithinRange).
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.Positive(0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.Positive(-1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.NonNegative(-1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Guard.NonNegative(-1m));
    }
}