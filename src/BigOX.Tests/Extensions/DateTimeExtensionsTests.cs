using System.Globalization;
using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class DateTimeExtensionsTests
{
    // GetNumberOfDaysInYear
    [TestMethod]
    public void GetNumberOfDaysInYear_UsesCalendarEraCorrectly()
    {
        Assert.AreEqual(366, DateTimeExtensions.GetNumberOfDaysInYear(2024));
        Assert.AreEqual(365, DateTimeExtensions.GetNumberOfDaysInYear(2023));

        // Verify custom culture calendar path doesn't throw and returns plausible value
        var jp = new CultureInfo("ja-JP");
        var days = DateTimeExtensions.GetNumberOfDaysInYear(2024, jp);
        Assert.IsTrue(days is 365 or 366);
    }

    // ToDateOnly / ToTimeOnly
    [TestMethod]
    public void ToDateOnly_DropsTime_PreservesDate()
    {
        var dt = new DateTime(2023, 1, 15, 10, 30, 45, 123);
        var d = dt.ToDateOnly();
        Assert.AreEqual(new DateOnly(2023, 1, 15), d);
    }

    [TestMethod]
    public void ToTimeOnly_DropsDate_PreservesTime()
    {
        var dt = new DateTime(2023, 1, 15, 10, 30, 45, 123);
        var t = dt.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(10, 30, 45, 123, dt.Microsecond), t);
    }

    // Age
    [TestMethod]
    public void Age_WithExplicitDate_ComputesYears()
    {
        var dob = new DateTime(2000, 2, 29);
        var maturity = new DateTime(2023, 2, 28);
        Assert.AreEqual(22, dob.Age(maturity));
    }

    [TestMethod]
    public void Age_WithTimeZone_UsesUtcConvertedToZone()
    {
        var dob = new DateTime(2000, 2, 29);
        var maturityUtc = new DateTime(2024, 2, 29, 10, 0, 0, DateTimeKind.Utc);
        var tz = TimeZoneInfo.Utc;
        Assert.AreEqual(24, dob.Age(maturityUtc, tz));
    }

    [TestMethod]
    public void Age_FutureBirthDate_Throws()
    {
        var futureDob = DateTime.Now.AddDays(1);
        Assert.ThrowsExactly<ArgumentException>(() => futureDob.Age());
    }

    // AddWeeks
    [TestMethod]
    public void AddWeeks_Fractional_RoundsUpToWholeDays()
    {
        var d = new DateTime(2023, 1, 1);
        Assert.AreEqual(new DateTime(2023, 1, 19), d.AddWeeks(2.5));
        Assert.AreEqual(new DateTime(2022, 12, 21), d.AddWeeks(-1.5));
    }

    // DaysInMonth
    [TestMethod]
    public void DaysInMonth_WorksLeapAndNonLeap()
    {
        Assert.AreEqual(28, new DateTime(2023, 2, 15).DaysInMonth());
        Assert.AreEqual(29, new DateTime(2024, 2, 15).DaysInMonth());
    }

    // First / Last of Month
    [TestMethod]
    public void GetFirstDateOfMonth_NoDay_ReturnsFirst()
    {
        var d = new DateTime(2023, 1, 15);
        Assert.AreEqual(new DateTime(2023, 1, 1), d.GetFirstDateOfMonth());
    }

    [TestMethod]
    public void GetFirstDateOfMonth_WithDay_SelectsFirstOccurrence()
    {
        var d = new DateTime(2023, 1, 15);
        Assert.AreEqual(new DateTime(2023, 1, 2), d.GetFirstDateOfMonth(DayOfWeek.Monday));
    }

    [TestMethod]
    public void GetLastDateOfMonth_NoDay_ReturnsLast()
    {
        var d = new DateTime(2023, 1, 15);
        Assert.AreEqual(new DateTime(2023, 1, 31), d.GetLastDateOfMonth());
    }

    [TestMethod]
    public void GetLastDateOfMonth_WithDay_SelectsLastOccurrence()
    {
        var d = new DateTime(2023, 1, 15);
        Assert.AreEqual(new DateTime(2023, 1, 27), d.GetLastDateOfMonth(DayOfWeek.Friday));
    }

    // First / Last of Week
    [TestMethod]
    public void GetFirstAndLastDateOfWeek_RespectsCulture()
    {
        var d = new DateTime(2023, 1, 18); // Wednesday
        var enUS = new CultureInfo("en-US"); // Sunday start
        var deDE = new CultureInfo("de-DE"); // Monday start
        Assert.AreEqual(new DateTime(2023, 1, 15), d.GetFirstDateOfWeek(enUS));
        Assert.AreEqual(new DateTime(2023, 1, 21), d.GetLastDateOfWeek(enUS));
        Assert.AreEqual(new DateTime(2023, 1, 16), d.GetFirstDateOfWeek(deDE));
        Assert.AreEqual(new DateTime(2023, 1, 22), d.GetLastDateOfWeek(deDE));
    }

    // Differences
    [TestMethod]
    public void GetNumberOfDays_SignedDifference()
    {
        var a = new DateTime(2023, 3, 1);
        var b = new DateTime(2023, 3, 28);
        Assert.AreEqual(27, a.GetNumberOfDays(b));
        Assert.AreEqual(-27, b.GetNumberOfDays(a));
        Assert.AreEqual(0, a.GetNumberOfDays(a));
    }

    // Comparisons
    [TestMethod]
    public void IsAfter_IsBefore_Work()
    {
        var a = new DateTime(2024, 1, 1);
        var b = new DateTime(2024, 1, 2);
        Assert.IsTrue(b.IsAfter(a));
        Assert.IsFalse(a.IsAfter(b));
        Assert.IsTrue(a.IsBefore(b));
        Assert.IsFalse(b.IsBefore(a));
    }

    [TestMethod]
    public void IsBetween_InclusiveExclusive()
    {
        var d = new DateTime(2023, 1, 15);
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 31);
        Assert.IsTrue(d.IsBetween(start, end));
        Assert.IsTrue(d.IsBetween(start, d));
        Assert.IsTrue(d.IsBetween(d, end));
        Assert.IsTrue(d.IsBetween(end, start)); // inverted bounds should still work
        Assert.IsFalse(d.IsBetween(d, end, false));
        Assert.IsFalse(d.IsBetween(start, d, false));
    }

    [TestMethod]
    public void IsDateEqual_IsTimeEqual_Work()
    {
        var a = new DateTime(2023, 3, 28, 10, 30, 0);
        var b = new DateTime(2023, 3, 28, 15, 45, 0);
        Assert.IsTrue(a.IsDateEqual(b));

        var c = new DateTime(2023, 4, 5, 10, 30, 0);
        Assert.IsTrue(a.IsTimeEqual(c));
    }

    [TestMethod]
    public void IsToday_ComparesDateOnly()
    {
        var today = DateTime.Today;
        Assert.IsTrue(today.IsToday());
        Assert.IsFalse(today.AddDays(-1).IsToday());
        Assert.IsFalse(today.AddDays(1).IsToday());
    }

    // Leap day and Elapsed
    [TestMethod]
    public void IsLeapDay_TrueOnlyOnFeb29()
    {
        Assert.IsTrue(new DateTime(2020, 2, 29).IsLeapDay());
        Assert.IsFalse(new DateTime(2021, 2, 28).IsLeapDay());
        Assert.IsFalse(new DateTime(2020, 2, 28).IsLeapDay());
    }

    [TestMethod]
    public void Elapsed_IsNonNegativeForPast()
    {
        var past = DateTime.Now.AddSeconds(-1);
        Assert.IsTrue(past.Elapsed() >= TimeSpan.Zero);
    }

    // SetTime
    [TestMethod]
    public void SetTime_WithTimeSpan_SetsTimeOfDay()
    {
        var date = new DateTime(2024, 1, 2, 10, 0, 0);
        var result = date.SetTime(new TimeSpan(16, 30, 0));
        Assert.AreEqual(new DateTime(2024, 1, 2, 16, 30, 0), result);
    }

    [TestMethod]
    public void SetTime_WithComponents_SetsTimeOfDay()
    {
        var date = new DateTime(2024, 1, 2);
        var result = date.SetTime(16, 0, 0, 500);
        Assert.AreEqual(new DateTime(2024, 1, 2, 16, 0, 0, 500), result);
    }

    [TestMethod]
    public void SetTime_WithTimeOnly_SetsTimeOfDay()
    {
        var date = new DateTime(2024, 1, 2);
        var result = date.SetTime(new TimeOnly(7, 45, 30));
        Assert.AreEqual(new DateTime(2024, 1, 2, 7, 45, 30), result);
    }

    // Range enumeration
    [TestMethod]
    public void GetDatesInRange_AscendingAndDescending_Inclusive()
    {
        var start = new DateTime(2023, 1, 1);
        var end = new DateTime(2023, 1, 5);
        var asc = start.GetDatesInRange(end).ToList();
        CollectionAssert.AreEqual(new[]
        {
            new DateTime(2023, 1, 1),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 3),
            new DateTime(2023, 1, 4),
            new DateTime(2023, 1, 5)
        }, asc);

        var desc = end.GetDatesInRange(start).ToList();
        CollectionAssert.AreEqual(new[]
        {
            new DateTime(2023, 1, 5),
            new DateTime(2023, 1, 4),
            new DateTime(2023, 1, 3),
            new DateTime(2023, 1, 2),
            new DateTime(2023, 1, 1)
        }, desc);
    }

    // NextDay / PreviousDay
    [TestMethod]
    public void NextDay_PreviousDay_ShiftByOne()
    {
        var d = new DateTime(2024, 3, 15);
        Assert.AreEqual(new DateTime(2024, 3, 16), d.NextDay());
        Assert.AreEqual(new DateTime(2024, 3, 14), d.PreviousDay());
    }

    // Timestamp
    [TestMethod]
    public void GetTimestamp_UsesRoundTripFormat()
    {
        var dt = new DateTime(2024, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var s = dt.GetTimestamp();
        Assert.Contains("T", s);
        Assert.IsTrue(s.EndsWith("Z") || s.Contains("+") || s.Contains("-"));
    }
}