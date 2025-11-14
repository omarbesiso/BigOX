using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class ComparableExtensionsTests
{
    // --------------------
    // IsBetween<T>
    // --------------------

    [TestMethod]
    public void IsBetween_Int_Inclusive_TrueForBoundariesAndMiddle()
    {
        var v1 = 5;
        Assert.IsTrue(v1.IsBetween(1, 10));
        Assert.IsTrue(v1.IsBetween(5, 10)); // lower boundary inclusive
        Assert.IsTrue(v1.IsBetween(1, 5)); // upper boundary inclusive
        Assert.IsTrue(5.IsBetween(5, 5)); // both boundaries inclusive
    }

    [TestMethod]
    public void IsBetween_Int_Exclusive_FalseForEqualToBoundaries()
    {
        var v = 5;
        Assert.IsTrue(v.IsBetween(1, 10, false));
        Assert.IsFalse(v.IsBetween(5, 10, false));
        Assert.IsFalse(v.IsBetween(1, 5, false));
        Assert.IsFalse(5.IsBetween(5, 5, false));
    }

    [TestMethod]
    public void IsBetween_String_BasicScenarios()
    {
        var s = "m";
        Assert.IsTrue(s.IsBetween("a", "z"));
        Assert.IsTrue(s.IsBetween("m", "z")); // lower inclusive
        Assert.IsTrue(s.IsBetween("a", "m")); // upper inclusive
        Assert.IsFalse(s.IsBetween("m", "m", false));
    }

    [TestMethod]
    public void IsBetween_DateTime_WorksForStructComparables()
    {
        DateTime date = new(2024, 06, 15);
        Assert.IsTrue(date.IsBetween(new DateTime(2024, 01, 01), new DateTime(2024, 12, 31)));
        Assert.IsFalse(date.IsBetween(new DateTime(2025, 01, 01), new DateTime(2025, 12, 31)));
    }

    [TestMethod]
    public void IsBetween_BoundariesInverted_BehavesByDirectComparison_NoEnforcement()
    {
        var v = 5;
        // lower > upper -> direct comparisons yield false
        Assert.IsFalse(v.IsBetween(10, 1));
        Assert.IsFalse(v.IsBetween(10, 1, false));
    }

    // --------------------
    // Limit(T maximum)
    // --------------------

    [TestMethod]
    public void Limit_Max_Int_ReturnsMinOfValueAndMax()
    {
        Assert.AreEqual(8, 10.Limit(8));
        Assert.AreEqual(6, 6.Limit(8));
        Assert.AreEqual(8, 8.Limit(8));
    }

    [TestMethod]
    public void Limit_Max_DateTime_ReturnsMinOfValueAndMax()
    {
        var max = new DateTime(2024, 12, 31);
        Assert.AreEqual(max, new DateTime(2025, 01, 01).Limit(max));
        var within = new DateTime(2024, 06, 01);
        Assert.AreEqual(within, within.Limit(max));
    }

    [TestMethod]
    public void Limit_Max_NullValue_ThrowsArgumentNullException_ForReferenceType()
    {
        string? s = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => s!.Limit("zzz"));
    }

    [TestMethod]
    public void Limit_Max_NullMaximum_ThrowsArgumentNullException_ForReferenceType()
    {
        var s = "abc";
        Assert.ThrowsExactly<ArgumentNullException>(() => s.Limit(null!));
    }

    // --------------------
    // Limit(T minimum, T maximum)
    // --------------------

    [TestMethod]
    public void Limit_MinMax_Int_ClampsBelowAndAbove_AndKeepsWithin()
    {
        Assert.AreEqual(10, 5.Limit(10, 20)); // below -> min
        Assert.AreEqual(20, 25.Limit(10, 20)); // above -> max
        Assert.AreEqual(15, 15.Limit(10, 20)); // within
        Assert.AreEqual(10, 10.Limit(10, 20)); // equals min
        Assert.AreEqual(20, 20.Limit(10, 20)); // equals max
    }

    [TestMethod]
    public void Limit_MinMax_String_UsesDefaultComparer()
    {
        var resultWithin = "m".Limit("a", "z");
        Assert.AreEqual("m", resultWithin);

        var resultBelow = "A".Limit("b", "z"); // "A" < "b" in default comparer
        Assert.AreEqual("b", resultBelow);

        var resultAbove = "zzz".Limit("a", "m");
        Assert.AreEqual("m", resultAbove);
    }

    [TestMethod]
    public void Limit_MinMax_NullValue_ThrowsArgumentNullException()
    {
        string? s = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => s!.Limit("a", "b"));
    }

    [TestMethod]
    public void Limit_MinMax_NullMinimum_ThrowsArgumentNullException()
    {
        var s = "x";
        Assert.ThrowsExactly<ArgumentNullException>(() => s.Limit(null!, "z"));
    }

    [TestMethod]
    public void Limit_MinMax_NullMaximum_ThrowsArgumentNullException()
    {
        var s = "x";
        Assert.ThrowsExactly<ArgumentNullException>(() => s.Limit("a", null!));
    }

    [TestMethod]
    public void Limit_MinMax_InvertedBoundaries_ReturnsMinimumWhenValueLessThanMinimum()
    {
        // The implementation does not normalize min/max; it compares directly.
        // With min > max and value < min, it returns the (inverted) minimum.
        Assert.AreEqual(10, 7.Limit(10, 5));
    }
}