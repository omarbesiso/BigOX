using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
public class GuardTests_DateTime
{
    [TestMethod]
    public void InFuture_Returns_ForFutureUtc()
    {
        var dt = DateTime.UtcNow.AddSeconds(2);
        var result = Guard.InFuture(dt, TimeZoneInfo.Utc);
        Assert.AreEqual(dt, result);
    }

    [TestMethod]
    public void InFuture_Throws_ForPastUtc()
    {
        var dt = DateTime.UtcNow.AddSeconds(-2);
        TestUtils.Expect<ArgumentException>(() => Guard.InFuture(dt, TimeZoneInfo.Utc));
    }

    [TestMethod]
    public void InPast_Returns_ForPastUtc()
    {
        var dt = DateTime.UtcNow.AddSeconds(-2);
        var result = Guard.InPast(dt, TimeZoneInfo.Utc);
        Assert.AreEqual(dt, result);
    }

    [TestMethod]
    public void InPast_Throws_ForFutureUtc()
    {
        var dt = DateTime.UtcNow.AddSeconds(2);
        TestUtils.Expect<ArgumentException>(() => Guard.InPast(dt, TimeZoneInfo.Utc));
    }
}