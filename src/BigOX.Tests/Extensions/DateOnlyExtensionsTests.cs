using System.Globalization;
using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class DateOnlyExtensionsTests
{
    // PreviousDay / NextDay
    [TestMethod]
    public void PreviousDay_NormalCase_ReturnsDayBefore()
    {
        var d = new DateOnly(2024, 3, 15);
        var y = d.PreviousDay();
        Assert.AreEqual(new DateOnly(2024, 3, 14), y);
    }

    [TestMethod]
    public void PreviousDay_OnMin_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => DateOnly.MinValue.PreviousDay());
    }

    [TestMethod]
    public void NextDay_NormalCase_ReturnsDayAfter()
    {
        var d = new DateOnly(2024, 3, 15);
        var t = d.NextDay();
        Assert.AreEqual(new DateOnly(2024, 3, 16), t);
    }

    [TestMethod]
    public void NextDay_OnMax_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => DateOnly.MaxValue.NextDay());
    }

    // GetDatesInRange
    [TestMethod]
    public void GetDatesInRange_Ascending_Inclusive()
    {
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 1, 5);
        var list = start.GetDatesInRange(end).ToList();
        CollectionAssert.AreEqual(new[]
        {
            new DateOnly(2024, 1, 1),
            new DateOnly(2024, 1, 2),
            new DateOnly(2024, 1, 3),
            new DateOnly(2024, 1, 4),
            new DateOnly(2024, 1, 5)
        }, list);
    }

    [TestMethod]
    public void GetDatesInRange_Descending_Inclusive()
    {
        var start = new DateOnly(2024, 1, 5);
        var end = new DateOnly(2024, 1, 1);
        var list = start.GetDatesInRange(end).ToList();
        CollectionAssert.AreEqual(new[]
        {
            new DateOnly(2024, 1, 5),
            new DateOnly(2024, 1, 4),
            new DateOnly(2024, 1, 3),
            new DateOnly(2024, 1, 2),
            new DateOnly(2024, 1, 1)
        }, list);
    }

    [TestMethod]
    public void GetDatesInRange_SameDate_ReturnsSingle()
    {
        var d = new DateOnly(2024, 1, 5);
        var list = d.GetDatesInRange(d).ToList();
        CollectionAssert.AreEqual(new[] { d }, list);
    }

    // IsBetween
    [TestMethod]
    public void IsBetween_Inclusive_TrueOnEdgesAndInside()
    {
        var d = new DateOnly(2024, 6, 15);
        Assert.IsTrue(d.IsBetween(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31)));
        Assert.IsTrue(d.IsBetween(new DateOnly(2024, 6, 15), new DateOnly(2024, 12, 31)));
        Assert.IsTrue(d.IsBetween(new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 15)));
    }

    [TestMethod]
    public void IsBetween_Exclusive_FalseOnEdges()
    {
        var d = new DateOnly(2024, 6, 15);
        Assert.IsTrue(d.IsBetween(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31), false));
        Assert.IsFalse(d.IsBetween(new DateOnly(2024, 6, 15), new DateOnly(2024, 12, 31), false));
        Assert.IsFalse(d.IsBetween(new DateOnly(2024, 1, 1), new DateOnly(2024, 6, 15), false));
    }

    [TestMethod]
    public void IsBetween_InvertedRange_Handled()
    {
        var d = new DateOnly(2024, 6, 15);
        Assert.IsTrue(d.IsBetween(new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1)));
        Assert.IsTrue(d.IsBetween(new DateOnly(2024, 12, 31), new DateOnly(2024, 1, 1), false));
    }

    // ToDateTime
    [TestMethod]
    public void ToDateTime_DefaultMidnight_UnspecifiedKind()
    {
        var d = new DateOnly(2024, 1, 2);
        var dt = d.ToDateTime();
        Assert.AreEqual(new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Unspecified), dt);
    }

    [TestMethod]
    public void ToDateTime_WithTimeAndKind_RespectsArgs()
    {
        var d = new DateOnly(2024, 1, 2);
        var dt = d.ToDateTime(new TimeOnly(13, 30), DateTimeKind.Utc);
        Assert.AreEqual(new DateTime(2024, 1, 2, 13, 30, 0, DateTimeKind.Utc), dt);
    }

    [TestMethod]
    public void ToDateTime_WithDefaultTime_TreatedAsMidnight()
    {
        var d = new DateOnly(2024, 1, 2);
        var dt = d.ToDateTime(default, DateTimeKind.Local);
        Assert.AreEqual(new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Local), dt);
    }

    // Age
    [TestMethod]
    public void Age_WithExplicitMaturityDate_ComputesYears()
    {
        var dob = new DateOnly(2000, 2, 29);
        var maturity = new DateOnly(2023, 2, 28);
        Assert.AreEqual(22, dob.Age(maturity));
    }

    [TestMethod]
    public void Age_WithMaturityDateTime_AndTimeZone_ComputesYears()
    {
        var dob = new DateOnly(2000, 2, 29);
        var maturityUtc = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc);
        var tz = TimeZoneInfo.Utc; // deterministic
        Assert.AreEqual(24, dob.Age(maturityUtc, tz));
    }

    [TestMethod]
    public void Age_FutureBirthDate_ThrowsArgumentException()
    {
        var futureDob = new DateOnly(9999, 12, 31);
        Assert.ThrowsExactly<ArgumentException>(() => futureDob.Age(new DateOnly(2023, 1, 1)));
        Assert.ThrowsExactly<ArgumentException>(() => futureDob.Age(new DateTime(2023, 1, 1), TimeZoneInfo.Utc));
    }

    // AddWeeks
    [TestMethod]
    public void AddWeeks_Fractional_RoundsAwayFromZero()
    {
        var d = new DateOnly(2023, 1, 1);
        Assert.AreEqual(new DateOnly(2023, 1, 19), d.AddWeeks(2.5)); // 17.5 -> 18
        Assert.AreEqual(new DateOnly(2022, 12, 21), d.AddWeeks(-1.5)); // -10.5 -> -11
    }

    [TestMethod]
    public void AddWeeks_Zero_ReturnsSameDate()
    {
        var d = new DateOnly(2023, 1, 1);
        Assert.AreEqual(d, d.AddWeeks(0));
    }

    // DaysInMonth
    [TestMethod]
    public void DaysInMonth_WorksForLeapAndNonLeap()
    {
        Assert.AreEqual(28, new DateOnly(2023, 2, 15).DaysInMonth());
        Assert.AreEqual(29, new DateOnly(2024, 2, 15).DaysInMonth());
    }

    // GetFirstDateOfMonth
    [TestMethod]
    public void GetFirstDateOfMonth_NoDay_ReturnsFirst()
    {
        var d = new DateOnly(2023, 1, 15);
        Assert.AreEqual(new DateOnly(2023, 1, 1), d.GetFirstDateOfMonth());
    }

    [TestMethod]
    public void GetFirstDateOfMonth_WithDay_SelectsFirstOccurrence()
    {
        var d = new DateOnly(2023, 1, 15);
        Assert.AreEqual(new DateOnly(2023, 1, 2), d.GetFirstDateOfMonth(DayOfWeek.Monday));
    }

    // GetLastDateOfMonth
    [TestMethod]
    public void GetLastDateOfMonth_NoDay_ReturnsLast()
    {
        var d = new DateOnly(2023, 1, 15);
        Assert.AreEqual(new DateOnly(2023, 1, 31), d.GetLastDateOfMonth());
    }

    [TestMethod]
    public void GetLastDateOfMonth_WithDay_SelectsLastOccurrence()
    {
        var d = new DateOnly(2023, 1, 15);
        Assert.AreEqual(new DateOnly(2023, 1, 27), d.GetLastDateOfMonth(DayOfWeek.Friday));
    }

    // GetLastDateOfWeek
    [TestMethod]
    public void GetLastDateOfWeek_UsesCultureFirstDay()
    {
        var d = new DateOnly(2023, 1, 18); // Wednesday
        var enUS = new CultureInfo("en-US"); // Sunday start
        var deDE = new CultureInfo("de-DE"); // Monday start
        Assert.AreEqual(new DateOnly(2023, 1, 21), d.GetLastDateOfWeek(enUS));
        Assert.AreEqual(new DateOnly(2023, 1, 22), d.GetLastDateOfWeek(deDE));
    }

    // Comparisons and helpers
    [TestMethod]
    public void IsAfter_IsBefore_BehaveAsExpected()
    {
        var a = new DateOnly(2024, 1, 1);
        var b = new DateOnly(2024, 1, 2);
        Assert.IsTrue(b.IsAfter(a));
        Assert.IsFalse(a.IsAfter(b));
        Assert.IsTrue(a.IsBefore(b));
        Assert.IsFalse(b.IsBefore(a));
    }

    [TestMethod]
    public void IsToday_MatchesSystemToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        Assert.IsTrue(today.IsToday());
    }

    [TestMethod]
    public void LeapChecks_WorkForLeapDayAndYear()
    {
        Assert.IsTrue(new DateOnly(2020, 2, 29).IsLeapDay());
        Assert.IsFalse(new DateOnly(2021, 2, 28).IsLeapDay());
        Assert.IsTrue(new DateOnly(2020, 1, 1).IsLeapYear());
        Assert.IsFalse(new DateOnly(2021, 1, 1).IsLeapYear());
    }
}