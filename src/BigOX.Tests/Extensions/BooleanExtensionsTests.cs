using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class BooleanExtensionsTests
{
    [TestMethod]
    public void ToCustomString_Defaults_True_ReturnsTrue()
    {
        var result = true.ToCustomString();
        Assert.AreEqual("True", result);
    }

    [TestMethod]
    public void ToCustomString_Defaults_False_ReturnsFalse()
    {
        var result = false.ToCustomString();
        Assert.AreEqual("False", result);
    }

    [TestMethod]
    public void ToCustomString_CustomValues_True()
    {
        var result = true.ToCustomString("Yes", "No");
        Assert.AreEqual("Yes", result);
    }

    [TestMethod]
    public void ToCustomString_CustomValues_False()
    {
        var result = false.ToCustomString("Yes", "No");
        Assert.AreEqual("No", result);
    }

    [TestMethod]
    public void ToCustomString_OnlyTrueValueProvided_True_UsesCustomTrue_DefaultFalse()
    {
        var result = true.ToCustomString("Y");
        Assert.AreEqual("Y", result);
    }

    [TestMethod]
    public void ToCustomString_OnlyTrueValueProvided_False_UsesDefaultFalse()
    {
        var result = false.ToCustomString("Y");
        Assert.AreEqual("False", result);
    }

    [TestMethod]
    public void ToCustomString_NullTrueValue_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => true.ToCustomString(null!, "No"));
    }

    [TestMethod]
    public void ToCustomString_NullFalseValue_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => false.ToCustomString("Yes", null!));
    }

    [TestMethod]
    public void ToByte_True_Returns1()
    {
        Assert.AreEqual((byte)1, true.ToByte());
    }

    [TestMethod]
    public void ToByte_False_Returns0()
    {
        Assert.AreEqual((byte)0, false.ToByte());
    }

    [TestMethod]
    public void ToInt32_True_Returns1()
    {
        Assert.AreEqual(1, true.ToInt32());
    }

    [TestMethod]
    public void ToInt32_False_Returns0()
    {
        Assert.AreEqual(0, false.ToInt32());
    }
}