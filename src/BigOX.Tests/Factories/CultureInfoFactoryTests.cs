using System.Globalization;
using BigOX.Factories;

namespace BigOX.Tests.Factories;

[TestClass]
public sealed class CultureInfoFactoryTests
{
    [TestMethod]
    public void Create_ValidCulture_ReturnsReadOnlyCultureInfo()
    {
        var culture = CultureInfoFactory.Create("en-US");
        Assert.IsNotNull(culture);
        Assert.AreEqual("en-US", culture.Name);
        Assert.IsTrue(culture.IsReadOnly);
    }

    [TestMethod]
    public void Create_Null_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => CultureInfoFactory.Create(null!));
    }

    [TestMethod]
    public void Create_Empty_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CultureInfoFactory.Create(""));
    }

    [TestMethod]
    public void Create_Whitespace_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => CultureInfoFactory.Create("   "));
    }

    [TestMethod]
    public void Create_InvalidCulture_ThrowsCultureNotFoundException()
    {
        Assert.ThrowsExactly<CultureNotFoundException>(() => CultureInfoFactory.Create("xx-INVALID-yy"));
    }
}