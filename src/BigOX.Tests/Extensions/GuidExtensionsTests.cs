using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class GuidExtensionsTests
{
    [TestMethod]
    public void IsEmpty_GuidEmpty_ReturnsTrue()
    {
        Assert.IsTrue(Guid.Empty.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_GuidNewGuid_ReturnsFalse()
    {
        Assert.IsFalse(Guid.NewGuid().IsEmpty());
    }

    [TestMethod]
    public void IsNotEmpty_GuidEmpty_ReturnsFalse()
    {
        Assert.IsFalse(Guid.Empty.IsNotEmpty());
    }

    [TestMethod]
    public void IsNotEmpty_GuidNewGuid_ReturnsTrue()
    {
        Assert.IsTrue(Guid.NewGuid().IsNotEmpty());
    }

    [TestMethod]
    public void IsEmpty_And_IsNotEmpty_AreComplementary_ForSampleGuid()
    {
        var g = Guid.NewGuid();
        Assert.AreEqual(g.IsEmpty(), !g.IsNotEmpty());
    }
}