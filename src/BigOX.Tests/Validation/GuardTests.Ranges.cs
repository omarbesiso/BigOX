using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
// ReSharper disable once InconsistentNaming
public class GuardTests_Ranges
{
    [TestMethod]
    [DataRow(5, 1)]
    [DataRow(0, 0)]
    public void Minimum_Returns_WhenGte(int value, int min)
    {
        var result = Guard.Minimum(value, min);
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [DataRow(0, 1)]
    [DataRow(-1, 0)]
    public void Minimum_Throws_WhenLessThan(int value, int min)
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.Minimum(value, min));
    }

    [TestMethod]
    [DataRow(1, 5)]
    [DataRow(5, 5)]
    public void Maximum_Returns_WhenLte(int value, int max)
    {
        var result = Guard.Maximum(value, max);
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [DataRow(6, 5)]
    [DataRow(1, 0)]
    public void Maximum_Throws_WhenGreaterThan(int value, int max)
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.Maximum(value, max));
    }

    [TestMethod]
    [DataRow(3, 1, 5)]
    [DataRow(1, 1, 5)]
    [DataRow(5, 1, 5)]
    public void WithinRange_Returns_WhenInside(int value, int min, int max)
    {
        var result = Guard.WithinRange(value, min, max);
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [DataRow(0, 1, 5)]
    [DataRow(6, 1, 5)]
    public void WithinRange_Throws_WhenOutside(int value, int min, int max)
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.WithinRange(value, min, max));
    }

    [TestMethod]
    public void WithinRange_Throws_WhenMinGreaterThanMax()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.WithinRange(1, 5, 1));
    }

    [TestMethod]
    public void Requires_Throws_WhenPredicateNull()
    {
        TestUtils.Expect<ArgumentNullException>(() => Guard.Requires(5, null!));
    }

    [TestMethod]
    public void Requires_Returns_WhenPredicateTrue()
    {
        var result = Guard.Requires(5, n => n > 0);
        Assert.AreEqual(5, result);
    }

    [TestMethod]
    public void Requires_Throws_WhenPredicateFalse()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.Requires(5, n => n < 0));
    }
}