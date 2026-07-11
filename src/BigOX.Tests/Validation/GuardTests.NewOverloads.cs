using System.Text.RegularExpressions;
using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
public sealed class GuardTests_NewOverloads
{
    // -------------------------------------------------------- MatchesRegex(Regex)

    [TestMethod]
    public void MatchesRegex_Regex_NullValue_ReturnsNull()
    {
        var regex = new Regex("^[a-z]+$");
        Assert.IsNull(Guard.MatchesRegex(null, regex));
    }

    [TestMethod]
    public void MatchesRegex_Regex_Matching_ReturnsValue()
    {
        var regex = new Regex("^[a-z]+$");
        Assert.AreEqual("abc", Guard.MatchesRegex("abc", regex));
    }

    [TestMethod]
    public void MatchesRegex_Regex_NonMatching_ThrowsArgumentException()
    {
        var regex = new Regex("^[a-z]+$");
        TestUtils.Expect<ArgumentException>(() => Guard.MatchesRegex("ABC", regex));
    }

    [TestMethod]
    public void MatchesRegex_Regex_NullRegex_ThrowsArgumentNullException()
    {
        var ex = TestUtils.Expect<ArgumentNullException>(() => Guard.MatchesRegex("abc", (Regex)null!));
        StringAssert.Contains(ex.ParamName, "regex");
    }

    // ------------------------------------------------ MatchesRegex(pattern, timeout)

    [TestMethod]
    public void MatchesRegex_Timeout_Matching_ReturnsValue()
    {
        Assert.AreEqual("abc", Guard.MatchesRegex("abc", "^[a-z]+$", TimeSpan.FromSeconds(5)));
    }

    [TestMethod]
    public void MatchesRegex_Timeout_NonMatching_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.MatchesRegex("ABC", "^[a-z]+$", TimeSpan.FromSeconds(5)));
    }

    [TestMethod]
    public void MatchesRegex_Timeout_NullValue_ReturnsNull()
    {
        Assert.IsNull(Guard.MatchesRegex(null, "^[a-z]+$", TimeSpan.FromSeconds(5)));
    }

    [TestMethod]
    public void MatchesRegex_Timeout_EmptyPattern_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.MatchesRegex("abc", string.Empty, TimeSpan.FromSeconds(5)));
    }

    // ------------------------------------------------------ Url(allowedSchemes)

    [TestMethod]
    public void Url_AllowedSchemes_NullValue_ReturnsNull()
    {
        Assert.IsNull(Guard.Url(null, ["http", "https"]));
    }

    [TestMethod]
    public void Url_AllowedSchemes_AllowedScheme_ReturnsValue()
    {
        Assert.AreEqual("ftp://host/file", Guard.Url("ftp://host/file", ["ftp", "sftp"]));
    }

    [TestMethod]
    public void Url_AllowedSchemes_CaseInsensitiveMatch_ReturnsValue()
    {
        // Uri lower-cases the scheme; the comparison is OrdinalIgnoreCase either way.
        Assert.AreEqual("HTTPS://host", Guard.Url("HTTPS://host", ["https"]));
    }

    [TestMethod]
    public void Url_AllowedSchemes_DisallowedScheme_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.Url("http://host", ["https"]));
    }

    [TestMethod]
    public void Url_AllowedSchemes_RelativeUri_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.Url("/relative/path", ["http", "https"]));
    }

    [TestMethod]
    public void Url_AllowedSchemes_NullSchemes_ThrowsArgumentNullException()
    {
        var ex = TestUtils.Expect<ArgumentNullException>(() => Guard.Url("http://host", (string[])null!));
        StringAssert.Contains(ex.ParamName, "allowedSchemes");
    }

    [TestMethod]
    public void Url_AllowedSchemes_EmptySchemes_ThrowsArgumentException()
    {
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.Url("http://host", []));
        StringAssert.Contains(ex.ParamName, "allowedSchemes");
    }

    // ------------------------------------------------------ Span length overloads

    [TestMethod]
    public void MaxLength_Span_WithinLimit_ReturnsSpan()
    {
        var result = Guard.MaxLength("abc".AsSpan(), 5);
        Assert.AreEqual("abc", result.ToString());
    }

    [TestMethod]
    public void MaxLength_Span_Exceeds_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.MaxLength("abcd".AsSpan(), 2));
    }

    [TestMethod]
    public void MaxLength_Span_NegativeLimit_ThrowsArgumentOutOfRangeException()
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.MaxLength("a".AsSpan(), -1));
    }

    [TestMethod]
    public void MinLength_Span_MeetsMinimum_ReturnsSpan()
    {
        var result = Guard.MinLength("abc".AsSpan(), 2);
        Assert.AreEqual("abc", result.ToString());
    }

    [TestMethod]
    public void MinLength_Span_TooShort_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.MinLength("a".AsSpan(), 3));
    }

    [TestMethod]
    public void MinLength_Span_NegativeLimit_ThrowsArgumentOutOfRangeException()
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.MinLength("a".AsSpan(), -1));
    }

    [TestMethod]
    public void ExactLength_Span_ExactMatch_ReturnsSpan()
    {
        var result = Guard.ExactLength("abc".AsSpan(), 3);
        Assert.AreEqual("abc", result.ToString());
    }

    [TestMethod]
    public void ExactLength_Span_Mismatch_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.ExactLength("ab".AsSpan(), 3));
    }

    [TestMethod]
    public void ExactLength_Span_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        TestUtils.Expect<ArgumentOutOfRangeException>(() => Guard.ExactLength("a".AsSpan(), -1));
    }

    [TestMethod]
    public void LengthWithinRange_Span_Inside_ReturnsSpan()
    {
        var result = Guard.LengthWithinRange("abc".AsSpan(), 1, 5);
        Assert.AreEqual("abc", result.ToString());
    }

    [TestMethod]
    public void LengthWithinRange_Span_Outside_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("abcd".AsSpan(), 1, 3));
    }

    [TestMethod]
    public void LengthWithinRange_Span_InvalidConfig_ThrowsArgumentException()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("a".AsSpan(), 1, 0));
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("a".AsSpan(), -1, 3));
        TestUtils.Expect<ArgumentException>(() => Guard.LengthWithinRange("a".AsSpan(), 3, 1));
    }
}
