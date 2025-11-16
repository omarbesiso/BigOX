using BigOX.Internals;

namespace BigOX.Tests.Internals;

[TestClass]
public sealed class ThrowHelperTests
{
    [TestMethod]
    public void ThrowArgumentNull_DefaultMessage_SetsParamName_And_DefaultText()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(() => ThrowHelper.ThrowArgumentNull("value"));
        Assert.AreEqual("value", ex.ParamName);
        StringAssert.Contains(ex.Message, "The value of 'value' cannot be null.");
    }

    [TestMethod]
    public void ThrowArgumentNull_CustomMessage_SetsParamName_And_UsesCustomMessage()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(() => ThrowHelper.ThrowArgumentNull("x", "oops"));
        Assert.AreEqual("x", ex.ParamName);
        StringAssert.Contains(ex.Message, "oops");
    }

    [TestMethod]
    public void ThrowArgument_DefaultMessage_SetsParamName_And_DefaultText()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => ThrowHelper.ThrowArgument("p"));
        Assert.AreEqual("p", ex.ParamName);
        StringAssert.Contains(ex.Message, "The value of 'p' is invalid.");
    }

    [TestMethod]
    public void ThrowArgument_CustomMessage_SetsParamName_And_UsesCustomMessage()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => ThrowHelper.ThrowArgument("name", "bad arg"));
        Assert.AreEqual("name", ex.ParamName);
        StringAssert.Contains(ex.Message, "bad arg");
    }

    [TestMethod]
    public void ThrowArgumentOutOfRange_DefaultMessage_SetsParamName_And_DefaultText()
    {
        var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ThrowHelper.ThrowArgumentOutOfRange("count"));
        Assert.AreEqual("count", ex.ParamName);
        StringAssert.Contains(ex.Message, "The value of 'count' is outside the allowable range.");
    }

    [TestMethod]
    public void ThrowArgumentOutOfRange_CustomMessage_SetsParamName_And_UsesCustomMessage()
    {
        var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            ThrowHelper.ThrowArgumentOutOfRange("age", "too old"));
        Assert.AreEqual("age", ex.ParamName);
        StringAssert.Contains(ex.Message, "too old");
    }

    [TestMethod]
    public void ThrowArgumentOutOfRange_WithActualValue_SetsParamName_ActualValue_And_Message()
    {
        var ex = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            ThrowHelper.ThrowArgumentOutOfRange("limit", 123, "must be <= 10"));
        Assert.AreEqual("limit", ex.ParamName);
        Assert.AreEqual(123, ex.ActualValue);
        StringAssert.Contains(ex.Message, "must be <= 10");
    }
}