using BigOX.Tests.Validation;
using BigOX.Types;

namespace BigOX.Tests.Types;

[TestClass]
public sealed class DateRangeExtensionsTests
{
    [TestMethod]
    public void Duration_ReturnsInclusiveDays_ForClosedRange()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var days = r.Duration();
        Assert.AreEqual(10, days);
    }

    [TestMethod]
    public void Duration_Throws_ForOpenEndedRange()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1));
        TestUtils.Expect<InvalidOperationException>(() => r.Duration());
    }

    [TestMethod]
    public void TryGetDuration_ReturnsTrue_ForClosedRange()
    {
        var r = new DateRange(new DateOnly(2024, 2, 1), new DateOnly(2024, 2, 29));
        var ok = r.TryGetDuration(out var days);
        Assert.IsTrue(ok);
        Assert.AreEqual(29, days);
    }

    [TestMethod]
    public void TryGetDuration_ReturnsFalse_ForOpenEndedRange()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1));
        var ok = r.TryGetDuration(out var days);
        Assert.IsFalse(ok);
        Assert.AreEqual(0, days);
    }

    [TestMethod]
    public void Contains_IsInclusive_And_UsesEffectiveEnd()
    {
        var closed = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        Assert.IsTrue(closed.Contains(new DateOnly(2024, 1, 1)));
        Assert.IsTrue(closed.Contains(new DateOnly(2024, 1, 10)));
        Assert.IsFalse(closed.Contains(new DateOnly(2023, 12, 31)));
        Assert.IsFalse(closed.Contains(new DateOnly(2024, 1, 11)));

        var open = new DateRange(new DateOnly(2024, 1, 1));
        Assert.IsTrue(open.Contains(new DateOnly(9999, 12, 31)));
    }

    [TestMethod]
    public void Overlaps_Works_ForClosed_And_OpenEnded()
    {
        var a = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var b = new DateRange(new DateOnly(2024, 1, 5), new DateOnly(2024, 1, 15));
        var c = new DateRange(new DateOnly(2024, 1, 11), new DateOnly(2024, 1, 20));
        Assert.IsTrue(a.Overlaps(b));
        Assert.IsFalse(a.Overlaps(c));

        var open = new DateRange(new DateOnly(2024, 1, 1));
        var farFuture = new DateRange(new DateOnly(3000, 1, 1), new DateOnly(3000, 1, 10));
        Assert.IsTrue(open.Overlaps(farFuture));
    }

    [TestMethod]
    public void Intersection_ReturnsCorrectRange_OrNull()
    {
        var a = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));
        var b = new DateRange(new DateOnly(2024, 1, 5), new DateOnly(2024, 1, 15));
        var inter = a.Intersection(b);
        Assert.IsNotNull(inter);
        Assert.AreEqual(new DateOnly(2024, 1, 5), inter.Value.StartDate);
        Assert.AreEqual(new DateOnly(2024, 1, 10), inter.Value.EndDate);

        var c = new DateRange(new DateOnly(2024, 1, 11), new DateOnly(2024, 1, 20));
        Assert.IsNull(a.Intersection(c));
    }

    [TestMethod]
    public void Intersection_WithOpenEnded_ProducesExpectedEnds()
    {
        var open = new DateRange(new DateOnly(2024, 1, 1));
        var closed = new DateRange(new DateOnly(2024, 2, 1), new DateOnly(2024, 2, 10));
        var inter1 = open.Intersection(closed);
        Assert.IsNotNull(inter1);
        Assert.AreEqual(closed, inter1.Value);

        var open2 = new DateRange(new DateOnly(2024, 2, 1));
        var inter2 = open.Intersection(open2);
        Assert.IsNotNull(inter2);
        Assert.AreEqual(new DateOnly(2024, 2, 1), inter2.Value.StartDate);
        Assert.IsNull(inter2.Value.EndDate);
    }

    [TestMethod]
    public void GetWeeksInRange_Closed_SplitsIntoContiguousChunks()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 15));
        var chunks = r.GetWeeksInRange().ToList();
        Assert.HasCount(3, chunks);
        Assert.AreEqual(new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7)), chunks[0]);
        Assert.AreEqual(new DateRange(new DateOnly(2024, 1, 8), new DateOnly(2024, 1, 14)), chunks[1]);
        Assert.AreEqual(new DateRange(new DateOnly(2024, 1, 15), new DateOnly(2024, 1, 15)), chunks[2]);
    }

    [TestMethod]
    public void GetWeeksInRange_OpenEnded_RequiresMaxWeeks()
    {
        var open = new DateRange(new DateOnly(2024, 1, 1));
        TestUtils.Expect<ArgumentNullException>(() => open.GetWeeksInRange().ToList());

        var two = open.GetWeeksInRange(2).ToList();
        Assert.HasCount(2, two);
        Assert.AreEqual(new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7)), two[0]);
        Assert.AreEqual(new DateRange(new DateOnly(2024, 1, 8), new DateOnly(2024, 1, 14)), two[1]);
    }

    [TestMethod]
    public void GetWeeksInRange_Closed_NegativeMaxWeeks_Throws()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 7));
        TestUtils.Expect<ArgumentException>(() => r.GetWeeksInRange(-1).ToList());
    }

    [TestMethod]
    public void EnumerateDays_Closed_YieldsInclusiveDays()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 3));
        var days = r.EnumerateDays().ToList();
        CollectionAssert.AreEqual(new[]
        {
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 2),
            new DateOnly(2024, 1, 3)
        }, days);
    }

    [TestMethod]
    public void EnumerateDays_MaxCount_LimitsResults()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1));
        var days = r.EnumerateDays(5).ToList();
        CollectionAssert.AreEqual(new[]
        {
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 2),
            new DateOnly(2024, 1, 3),
            new DateOnly(2024, 1, 4),
            new DateOnly(2024, 1, 5)
        }, days);
    }

    [TestMethod]
    public void EnumerateDays_ZeroMaxCount_YieldsNothing()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1));
        var days = r.EnumerateDays(0).ToList();
        Assert.IsEmpty(days);
    }

    [TestMethod]
    public void EnumerateDays_NegativeMaxCount_Throws()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1));
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        TestUtils.Expect<ArgumentOutOfRangeException>(() => r.EnumerateDays(-1).ToList());
    }
}