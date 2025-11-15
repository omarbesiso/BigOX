using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class StringExtensionsTests
{
    [TestMethod]
    public void IsGuid_ValidAndInvalid()
    {
        var valid = "3F2504E0-4F89-11D3-9A0C-0305E82C3301";
        var invalid = "not-a-guid";

        Assert.IsTrue(valid.IsGuid);
        Assert.IsFalse(invalid.IsGuid);
        Assert.IsFalse(((string?)null).IsGuid);
    }

    [TestMethod]
    public void IsValidEmail_ValidAndInvalid()
    {
        var ok = "user@example.com";
        var bad1 = "invalid@";
        var bad2 = "@example.com";
        string? none = null;

        Assert.IsTrue(ok.IsValidEmail);
        Assert.IsFalse(bad1.IsValidEmail);
        Assert.IsFalse(bad2.IsValidEmail);
        Assert.IsFalse(none.IsValidEmail);
    }

    [TestMethod]
    public void IsValidWebsiteUrl_HttpHttpsOnly()
    {
        var http = "http://example.com";
        var https = "https://example.com/path?q=1";
        var ftp = "ftp://example.com";
        var relative = "/path";
        string? none = null;

        Assert.IsTrue(http.IsValidWebsiteUrl);
        Assert.IsTrue(https.IsValidWebsiteUrl);
        Assert.IsFalse(ftp.IsValidWebsiteUrl);
        Assert.IsFalse(relative.IsValidWebsiteUrl);
        Assert.IsFalse(none.IsValidWebsiteUrl);
    }

    [TestMethod]
    public void ExtractDigits_ReturnsOnlyDigits_OrEmpty()
    {
        var s = "a1b2c3";
        Assert.AreEqual("123", s.ExtractDigits());
        Assert.AreEqual("", ((string?)"no-digits").ExtractDigits());
        Assert.AreEqual("", ((string?)null).ExtractDigits());
    }

    [TestMethod]
    public void ReduceToLength_Truncates_And_ThrowsOnNull()
    {
        Assert.AreEqual("Hello", ((string?)"Hello, World").ReduceToLength(5));
        Assert.AreEqual("Hello", ((string?)"Hello").ReduceToLength(5));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((string?)null)!.ReduceToLength(3));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((string?)"x").ReduceToLength(-1));
    }

    [TestMethod]
    public void EnsureStartsWith_AddsPrefix_WhenMissing_And_Throws_OnNulls()
    {
        Assert.AreEqual("Hello, World", ((string?)"World").EnsureStartsWith("Hello, "));
        Assert.AreEqual("Hello, World", ((string?)"Hello, World").EnsureStartsWith("Hello, "));

        Assert.ThrowsExactly<ArgumentNullException>(() => ((string?)null)!.EnsureStartsWith("Hi"));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((string?)"text").EnsureStartsWith(null!));
    }

    [TestMethod]
    public void EnsureEndsWith_AddsSuffix_WhenMissing_And_Throws_OnNulls()
    {
        Assert.AreEqual("Hello, World", ((string?)"Hello").EnsureEndsWith(", World"));
        Assert.AreEqual("Hello, World", ((string?)"Hello, World").EnsureEndsWith(", World"));

        Assert.ThrowsExactly<ArgumentNullException>(() => ((string?)null)!.EnsureEndsWith("!"));
        Assert.ThrowsExactly<ArgumentNullException>(() => ((string?)"text").EnsureEndsWith(null!));
    }

    [TestMethod]
    public void RemoveWhitespace_RemovesAll_And_HandlesNullOrEmpty()
    {
        Assert.AreEqual("HelloWorld", ((string?)"  Hello  World  ").RemoveWhitespace());
        Assert.AreEqual("", ((string?)"    ").RemoveWhitespace());
        Assert.AreEqual("", ((string?)string.Empty).RemoveWhitespace());
        Assert.IsNull(((string?)null).RemoveWhitespace());
    }

    [TestMethod]
    public void ToStringBuilder_WrapsString_HandlesNull()
    {
        var sb = ((string?)"abc").ToStringBuilder();
        Assert.AreEqual("abc", sb.ToString());

        var emptyBuilder = ((string?)null).ToStringBuilder();
        Assert.AreEqual(string.Empty, emptyBuilder.ToString());
    }

    [TestMethod]
    public void AppendCharToLength_PadsOrReturnsOriginal()
    {
        Assert.AreEqual("Hello-----", ((string?)"Hello").AppendCharToLength(10, '-'));
        Assert.AreEqual("Hello", ((string?)"Hello").AppendCharToLength(5, '-'));
        Assert.AreEqual("", ((string?)"").AppendCharToLength(0, '*'));

        string? none = null;
        Assert.AreEqual("*****", none.AppendCharToLength(5, '*'));
    }

    [TestMethod]
    public void IsDateTime_ParsesOrNot()
    {
        Assert.IsTrue(((string?)"2024-01-01").IsDateTime());
        Assert.IsTrue(((string?)"January 1, 2024").IsDateTime());
        Assert.IsFalse(((string?)"not a date").IsDateTime());
        Assert.IsFalse(((string?)null).IsDateTime());
    }

    [TestMethod]
    public void LimitLength_RespectsMax_ReturnsNull_ForNull()
    {
        Assert.AreEqual("abc", ((string?)"abcdef").LimitLength(3));
        Assert.AreEqual("abc", ((string?)"abc").LimitLength(3));
        Assert.AreEqual("", ((string?)"").LimitLength(2));
        Assert.IsNull(((string?)null).LimitLength(5));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ((string?)"x").LimitLength(-1));
    }
}