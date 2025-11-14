using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
// ReSharper disable once InconsistentNaming
public class GuardTests_PatternsAndUris
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("https://example.com")]
    [DataRow("http://example.com/path?q=1#hash")]
    public void Url_AllowsNull_And_ValidUrls(string? url)
    {
        var result = Guard.Url(url);
        Assert.AreEqual(url, result);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("notaurl")]
    [DataRow("ftp://example.com")]
    public void Url_Throws_ForInvalid(string url)
    {
        TestUtils.Expect<ArgumentException>(() => Guard.Url(url));
    }

    [TestMethod]
    public void Url_CustomMessage_IsUsed_OnInvalid()
    {
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.Url("x", exceptionMessage: "custom-url"));
        StringAssert.Contains(ex.Message, "custom-url");
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("user@example.com")]
    [DataRow("first.last+tag@sub.domain.example")]
    public void EmailAddress_AllowsNull_And_Valid(string? email)
    {
        var result = Guard.EmailAddress(email);
        Assert.AreEqual(email, result);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("not-an-email")]
    public void EmailAddress_Throws_ForInvalid(string email)
    {
        TestUtils.Expect<ArgumentException>(() => Guard.EmailAddress(email));
    }

    [TestMethod]
    public void EmailAddress_CustomMessage_IsUsed_OnInvalid()
    {
        var ex = TestUtils.Expect<ArgumentException>(() =>
            Guard.EmailAddress("not-an-email", exceptionMessage: "custom-email"));
        StringAssert.Contains(ex.Message, "custom-email");
    }

    [TestMethod]
    [DataRow(null, @"^[a-z]*$")]
    [DataRow("abc", @"^[a-z]*$")]
    public void MatchesRegex_AllowsNull_And_Matching(string? value, string pattern)
    {
        var result = Guard.MatchesRegex(value, pattern);
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    [DataRow("abc", @"^\d+$")]
    [DataRow("not-an-email", @"^[\w\.-]+@[\w\.-]+\.\w+$")]
    public void MatchesRegex_Throws_ForNonMatching(string value, string pattern)
    {
        TestUtils.Expect<ArgumentException>(() => Guard.MatchesRegex(value, pattern));
    }

    [TestMethod]
    public void MatchesRegex_CustomMessage_IsUsed_OnNonMatching()
    {
        var ex = TestUtils.Expect<ArgumentException>(() =>
            Guard.MatchesRegex("ABC", @"^[a-z]*$", exceptionMessage: "custom-regex"));
        StringAssert.Contains(ex.Message, "custom-regex");
    }

    [TestMethod]
    public void MatchesRegex_Throws_WhenPatternNullOrEmpty()
    {
        TestUtils.Expect<ArgumentNullException>(() => Guard.MatchesRegex("a", null!));
        TestUtils.Expect<ArgumentException>(() => Guard.MatchesRegex("a", ""));
    }
}