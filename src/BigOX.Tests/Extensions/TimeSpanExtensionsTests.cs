using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class TimeSpanExtensionsTests
{
    [TestMethod]
    public void ToTimeOnly_WithinDay_ReturnsSameTime()
    {
        var ts = new TimeSpan(13, 45, 30);
        var t = ts.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(13, 45, 30), t);
    }

    [TestMethod]
    public void ToTimeOnly_WrapsBeyond24Hours()
    {
        var ts = TimeSpan.FromHours(26); // 1 day + 2 hours
        var t = ts.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(2, 0, 0), t);
    }

    [TestMethod]
    public void ToTimeOnly_WrapsMultipleDaysAndKeepsMinutes()
    {
        var ts = TimeSpan.FromHours(49) + TimeSpan.FromMinutes(30); // 2 days + 1:30
        var t = ts.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(1, 30, 0), t);
    }

    [TestMethod]
    public void ToTimeOnly_Negative_WrapsToPositiveTimeOfDay()
    {
        var ts = TimeSpan.FromHours(-1); // -01:00 => 23:00
        var t = ts.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(23, 0, 0), t);
    }

    [TestMethod]
    public void ToTimeOnly_NegativeBeyond24_WrapsCorrectly()
    {
        var ts = TimeSpan.FromHours(-25) - TimeSpan.FromMinutes(30); // -25:30 => 22:30
        var t = ts.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(22, 30, 0), t);
    }

    [TestMethod]
    public void ToTimeOnly_ExactMultiplesOf24Hours_ReturnsMidnight()
    {
        Assert.AreEqual(new TimeOnly(0, 0), TimeSpan.FromDays(1).ToTimeOnly());
        Assert.AreEqual(new TimeOnly(0, 0), TimeSpan.FromDays(3).ToTimeOnly());
        Assert.AreEqual(new TimeOnly(0, 0), TimeSpan.FromDays(-2).ToTimeOnly());
    }

    [TestMethod]
    public void ToTimeOnly_PreservesSubSecondPrecision()
    {
        var baseTs = TimeSpan.FromHours(1) + TimeSpan.FromMinutes(2) + TimeSpan.FromSeconds(3) +
                     TimeSpan.FromMilliseconds(4);
        var ts = TimeSpan.FromDays(1) + baseTs + TimeSpan.FromTicks(42); // ensures tick-level precision
        var t = ts.ToTimeOnly();
        var expected = TimeOnly.FromTimeSpan(baseTs + TimeSpan.FromTicks(42));
        Assert.AreEqual(expected, t);
    }

    [TestMethod]
    public void Nullable_ToTimeOnly_Null_ReturnsNull()
    {
        TimeSpan? ts = null;
        Assert.IsNull(ts.ToTimeOnly());
    }

    [TestMethod]
    public void Nullable_ToTimeOnly_Value_DelegatesToNonNull()
    {
        TimeSpan? ts = TimeSpan.FromHours(26.5); // 02:30
        var t = ts.ToTimeOnly();
        Assert.AreEqual(new TimeOnly(2, 30), t);
    }
}