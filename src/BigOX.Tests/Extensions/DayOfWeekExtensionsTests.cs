using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class DayOfWeekExtensionsTests
{
    // AddDays
    [TestMethod]
    public void AddDays_Default_AddsOne()
    {
        var result = DayOfWeek.Monday.AddDays();
        Assert.AreEqual(DayOfWeek.Tuesday, result);
    }

    [TestMethod]
    public void AddDays_Positive_WrapsModulo7()
    {
        var result = DayOfWeek.Saturday.AddDays(2);
        Assert.AreEqual(DayOfWeek.Monday, result);
    }

    [TestMethod]
    public void AddDays_Negative_WrapsModulo7()
    {
        var result = DayOfWeek.Monday.AddDays(-1);
        Assert.AreEqual(DayOfWeek.Sunday, result);
    }

    [TestMethod]
    public void AddDays_MultipleOf7_ReturnsSame()
    {
        Assert.AreEqual(DayOfWeek.Wednesday, DayOfWeek.Wednesday.AddDays(14));
        Assert.AreEqual(DayOfWeek.Friday, DayOfWeek.Friday.AddDays(-7));
    }

    [TestMethod]
    public void AddDays_LargeOffsets_PositiveAndNegative()
    {
        // 100 % 7 = 2
        Assert.AreEqual(DayOfWeek.Wednesday, DayOfWeek.Monday.AddDays(100));
        // -100 normalizes to +5 steps from Monday => Saturday
        Assert.AreEqual(DayOfWeek.Saturday, DayOfWeek.Monday.AddDays(-100));
    }

    // GetNextDays
    [TestMethod]
    public void GetNextDays_Default7_FullWeekFromStart()
    {
        var start = DayOfWeek.Thursday;
        var expected = new[]
        {
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday,
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday
        };
        var list = start.GetNextDays().ToList();
        CollectionAssert.AreEqual(expected, list);
    }

    [TestMethod]
    public void GetNextDays_Count3_SequentialFromStart()
    {
        var list = DayOfWeek.Monday.GetNextDays(3).ToList();
        CollectionAssert.AreEqual(new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday }, list);
    }

    [TestMethod]
    public void GetNextDays_Count1_ReturnsSingle()
    {
        var list = DayOfWeek.Sunday.GetNextDays(1).ToList();
        CollectionAssert.AreEqual(new[] { DayOfWeek.Sunday }, list);
    }

    [TestMethod]
    public void GetNextDays_Throws_WhenCountLessThan1()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => DayOfWeek.Monday.GetNextDays(0).ToList());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => DayOfWeek.Monday.GetNextDays(-5).ToList());
    }
}