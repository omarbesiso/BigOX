using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
// ReSharper disable once InconsistentNaming
public class GuardTests_Strings
{
    [TestMethod]
    public void NotNull_ReturnsValue_WhenNotNull()
    {
        var s = "abc";
        var result = Guard.NotNull(s);
        Assert.AreSame(s, result);
    }

    [TestMethod]
    public void NotNull_Throws_WhenNull()
    {
        string? s = null;
        var ex = TestUtils.Expect<ArgumentNullException>(() => Guard.NotNull(s));
        StringAssert.Contains(ex.ParamName, nameof(s));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void NotNullOrWhiteSpace_Throws_ForInvalid(string? value)
    {
        var ex = value is null
            ? TestUtils.Expect<ArgumentNullException>(() => Guard.NotNullOrWhiteSpace(value))
            : TestUtils.Expect<ArgumentException>(() => Guard.NotNullOrWhiteSpace(value));
        StringAssert.Contains(ex.ParamName, nameof(value));
    }

    [TestMethod]
    [DataRow("a")]
    [DataRow(" abc ")]
    public void NotNullOrWhiteSpace_Returns_ForValid(string value)
    {
        var result = Guard.NotNullOrWhiteSpace(value);
        Assert.AreSame(value, result);
    }

    [TestMethod]
    public void NotNullOrWhiteSpace_CustomMessage_IsUsed_OnNull_And_Whitespace()
    {
        string? nullVal = null;
        var ex1 = TestUtils.Expect<ArgumentNullException>(() =>
            Guard.NotNullOrWhiteSpace(nullVal, exceptionMessage: "custom-null"));
        StringAssert.Contains(ex1.Message, "custom-null");

        var ex2 = TestUtils.Expect<ArgumentException>(() =>
            Guard.NotNullOrWhiteSpace("   ", exceptionMessage: "custom-ws"));
        StringAssert.Contains(ex2.Message, "custom-ws");
    }

    [TestMethod]
    public void NotNullOrEmpty_String_Throws_OnNull()
    {
        string? v = null;
        var ex = TestUtils.Expect<ArgumentNullException>(() => Guard.NotNullOrEmpty(v));
        StringAssert.Contains(ex.ParamName, nameof(v));
    }

    [TestMethod]
    public void NotNullOrEmpty_String_Throws_OnEmpty()
    {
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.NotNullOrEmpty(""));
        StringAssert.Contains(ex.ParamName, "\""); // param captured from expression
    }

    [TestMethod]
    public void NotNullOrEmpty_String_CustomMessage_IsUsed_OnNull_And_Empty()
    {
        string? n = null;
        var ex1 =
            TestUtils.Expect<ArgumentNullException>(() => Guard.NotNullOrEmpty(n, exceptionMessage: "custom-null"));
        StringAssert.Contains(ex1.Message, "custom-null");

        var ex2 = TestUtils.Expect<ArgumentException>(() =>
            Guard.NotNullOrEmpty(string.Empty, exceptionMessage: "custom-empty"));
        StringAssert.Contains(ex2.Message, "custom-empty");
    }

    [TestMethod]
    public void NotNullOrEmpty_String_Returns_OnNonEmpty()
    {
        var s = "x";
        var result = Guard.NotNullOrEmpty(s);
        Assert.AreSame(s, result);
    }

    [TestMethod]
    [DataRow(null, 0)]
    [DataRow(null, 3)]
    [DataRow("", 0)]
    [DataRow("abc", 3)]
    public void ExactLength_AllowsNull_And_EnforcesExact(string? value, int len)
    {
        if (value is null)
        {
            Assert.IsNull(Guard.ExactLength(value, len));
            return;
        }

        if (value.Length == len)
        {
            Assert.AreSame(value, Guard.ExactLength(value, len));
        }
        else
        {
            TestUtils.Expect<ArgumentException>(() => Guard.ExactLength(value, len));
        }
    }

    [TestMethod]
    public void ExactLength_Negative_Throws_OutOfRange()
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.ExactLength("a", -1));
    }

    [TestMethod]
    public void ExactLength_CustomMessage_IsUsed_OnMismatch()
    {
        var ex = TestUtils.Expect<ArgumentException>(() =>
            Guard.ExactLength("ab", 3, exceptionMessage: "custom-exact"));
        StringAssert.Contains(ex.Message, "custom-exact");
    }

    [TestMethod]
    [DataRow(null, 0)]
    [DataRow(null, 3)]
    [DataRow("", 0)]
    [DataRow("abc", 5)]
    public void MaxLength_AllowsNull_And_EnforcesMax(string? value, int max)
    {
        if (value is null)
        {
            Assert.IsNull(Guard.MaxLength(value, max));
            return;
        }

        if (value.Length <= max)
        {
            Assert.AreSame(value, Guard.MaxLength(value, max));
        }
        else
        {
            TestUtils.Expect<ArgumentException>(() => Guard.MaxLength(value, max));
        }
    }

    [TestMethod]
    public void MaxLength_Negative_Throws_OutOfRange()
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.MaxLength("a", -1));
    }

    [TestMethod]
    public void MaxLength_CustomMessage_IsUsed_OnTooLong()
    {
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.MaxLength("abcd", 3, exceptionMessage: "custom-max"));
        StringAssert.Contains(ex.Message, "custom-max");
    }

    [TestMethod]
    [DataRow(null, 0)]
    [DataRow(null, 3)]
    [DataRow("", 0)]
    [DataRow("abc", 2)]
    public void MinLength_AllowsNull_And_EnforcesMin(string? value, int min)
    {
        if (value is null)
        {
            Assert.IsNull(Guard.MinLength(value, min));
            return;
        }

        if (value.Length >= min)
        {
            Assert.AreSame(value, Guard.MinLength(value, min));
        }
        else
        {
            TestUtils.Expect<ArgumentException>(() => Guard.MinLength(value, min));
        }
    }

    [TestMethod]
    public void MinLength_Negative_Throws_OutOfRange()
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.MinLength("a", -1));
    }

    [TestMethod]
    public void MinLength_CustomMessage_IsUsed_OnTooShort()
    {
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.MinLength("a", 2, exceptionMessage: "custom-min"));
        StringAssert.Contains(ex.Message, "custom-min");
    }

    [TestMethod]
    [DataRow(null, 1, 3)]
    [DataRow("a", 1, 3)]
    [DataRow("abc", 1, 3)]
    public void LengthWithinRange_AllowsNull_And_PassesWhenInside(string? value, int min, int max)
    {
        var result = Guard.LengthWithinRange(value, min, max);
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void LengthWithinRange_Throws_WhenOutside()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("", 1, 3));
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("abcd", 1, 3));
    }

    [TestMethod]
    public void LengthWithinRange_CustomMessage_IsUsed_WhenOutside()
    {
        var ex = TestUtils.Expect<ArgumentException>(() =>
            Guard.LengthWithinRange("abcd", 1, 3, exceptionMessage: "custom-range"));
        StringAssert.Contains(ex.Message, "custom-range");
    }

    [TestMethod]
    public void LengthWithinRange_Throws_OnInvalidConfig()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("a", -1, 3));
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("a", 1, 0));
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("a", 3, 1));
    }
}