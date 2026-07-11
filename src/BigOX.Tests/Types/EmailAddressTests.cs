using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using BigOX.Types;

namespace BigOX.Tests.Types;

[TestClass]
public sealed class EmailAddressTests
{
    [TestMethod]
    public void DebuggerDisplay_FormatString_HasBalancedBraces()
    {
        // Regression guard for the malformed "{DebuggerDisplay,nq" (missing closing brace).
        var attr = typeof(EmailAddress).GetCustomAttribute<DebuggerDisplayAttribute>();
        Assert.IsNotNull(attr);
        var format = attr.Value;
        Assert.IsNotNull(format);
        Assert.AreEqual(format.Count(c => c == '{'), format.Count(c => c == '}'));
        Assert.EndsWith("}", format);
    }

    [TestMethod]
    public void DefaultValue_IsEmptySentinel()
    {
        var e = default(EmailAddress);
        Assert.IsTrue(e.IsEmpty);
        Assert.AreEqual(string.Empty, e.Address);
        Assert.IsNull(e.DisplayName);
        Assert.AreEqual(string.Empty, e.ToString());
    }

    [TestMethod]
    public void From_AddressOnly_NormalizesLowerCase()
    {
        var e = EmailAddress.From("User.NAME+Tag@Example.COM");
        Assert.AreEqual("user.name+tag@example.com", e.Address);
        Assert.IsNull(e.DisplayName);
    }

    [TestMethod]
    public void From_AddressAndDisplay_NormalizesAndTitleCases()
    {
        var e = EmailAddress.From("USER@EXAMPLE.COM", "  JOHN DOE  ");
        Assert.AreEqual("john doe", "JOHN DOE".ToLowerInvariant()); // sanity of approach
        Assert.AreEqual("user@example.com", e.Address);
        Assert.AreEqual("John Doe", e.DisplayName); // TitleCase applied
    }

    [TestMethod]
    public void From_DisplayWhitespace_BecomesNull()
    {
        var e = EmailAddress.From("a@b.com", "   \t  ");
        Assert.IsNull(e.DisplayName);
    }

    [TestMethod]
    public void From_InvalidAddress_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => EmailAddress.From("not-an-address"));
    }

    [TestMethod]
    public void From_InvalidAddressWithDisplay_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => EmailAddress.From("bad", "Name"));
    }

    [TestMethod]
    public void Parse_RawAddress_Succeeds()
    {
        var e = EmailAddress.Parse("MiXeD@Example.Com", null);
        Assert.AreEqual("mixed@example.com", e.Address);
        Assert.IsNull(e.DisplayName);
    }

    [TestMethod]
    public void Parse_CombinedForm_SucceedsAndNormalizes()
    {
        var e = EmailAddress.Parse("JANE DOE <Jane.Doe@Example.com>", CultureInfo.InvariantCulture);
        Assert.AreEqual("jane.doe@example.com", e.Address);
        Assert.AreEqual("Jane Doe", e.DisplayName);
        Assert.AreEqual("Jane Doe <jane.doe@example.com>", e.ToString());
    }

    [TestMethod]
    public void Parse_Whitespace_Trimmed()
    {
        var e = EmailAddress.Parse("  user@Example.com  ", null);
        Assert.AreEqual("user@example.com", e.Address);
    }

    [TestMethod]
    public void Parse_Invalid_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => EmailAddress.Parse(" ", null));
        Assert.ThrowsExactly<FormatException>(() => EmailAddress.Parse("invalid@@example.com", null));
    }

    [TestMethod]
    public void TryParse_Invalid_ReturnsFalse()
    {
        Assert.IsFalse(EmailAddress.TryParse(null, null, out _));
        Assert.IsFalse(EmailAddress.TryParse(" ", null, out _));
        Assert.IsFalse(EmailAddress.TryParse("invalid@@example.com", null, out _));
    }

    [TestMethod]
    public void TryParse_Combined_Succeeds()
    {
        Assert.IsTrue(EmailAddress.TryParse("John Smith <John.SMITH@Example.com>", CultureInfo.InvariantCulture,
            out var e));
        Assert.AreEqual("john.smith@example.com", e.Address);
        Assert.AreEqual("John Smith", e.DisplayName);
    }

    [TestMethod]
    public void ToString_Format_A_ReturnsAddressOnly()
    {
        var e = EmailAddress.From("USER@EXAMPLE.COM", "john smith");
        Assert.AreEqual("user@example.com", e.ToString("A", null));
        Assert.AreEqual("user@example.com", e.ToString("a", null));
    }

    [TestMethod]
    public void ToString_Format_F_ReturnsFullForm()
    {
        var e = EmailAddress.From("USER@EXAMPLE.COM", "john smith");
        var expected = "John Smith <user@example.com>";
        Assert.AreEqual(expected, e.ToString("F", null));
        Assert.AreEqual(expected, e.ToString("f", null));
    }

    [TestMethod]
    public void ToString_Format_G_DefaultEquivalentToFull()
    {
        var e = EmailAddress.From("USER@EXAMPLE.COM", "john smith");
        Assert.AreEqual(e.ToString("G", null), e.ToString());
        Assert.AreEqual("John Smith <user@example.com>", e.ToString());
    }

    [TestMethod]
    public void ToString_InvalidFormat_ThrowsFormatException()
    {
        var e = EmailAddress.From("a@b.com", "x");
        Assert.ThrowsExactly<FormatException>(() => e.ToString("Z", null));
    }

    [TestMethod]
    public void Equality_SameAddressDifferentDisplay_AreEqual()
    {
        var a = EmailAddress.From("user@example.com", "Alpha");
        var b = EmailAddress.From("USER@EXAMPLE.COM", "Beta");
        Assert.AreEqual(a, b);
        Assert.IsTrue(a.Equals(b));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_DifferentAddresses_NotEqual()
    {
        var a = EmailAddress.From("user1@example.com", "A");
        var b = EmailAddress.From("user2@example.com", "A");
        Assert.AreNotEqual(a, b);
        Assert.IsFalse(a.Equals(b));
    }

    [TestMethod]
    public void CompareTo_SortsByAddressThenDisplayNameOrdinal()
    {
        var a = EmailAddress.From("a@example.com", "Alpha");
        var b = EmailAddress.From("A@EXAMPLE.COM", "Beta"); // same address diff display
        var c = EmailAddress.From("b@example.com", "Alpha");
        // a and b have same address (case-insensitive); display names Alpha vs Beta => Alpha precedes Beta
        Assert.IsLessThan(0, a.CompareTo(b));
        Assert.IsGreaterThan(0, b.CompareTo(a));
        // Address ordering: a@example.com precedes b@example.com
        Assert.IsLessThan(0, a.CompareTo(c));
        Assert.IsGreaterThan(0, c.CompareTo(a));
    }

    [TestMethod]
    public void GetHashCode_CaseInsensitiveOnAddress()
    {
        var a = EmailAddress.From("User@Example.com");
        var b = EmailAddress.From("user@example.COM");
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Parse_WithCulture_AppliesTitleCase()
    {
        var culture = new CultureInfo("en-US");
        var e = EmailAddress.Parse("jOHN sMITH <John.Smith@Example.com>", culture);
        Assert.AreEqual("John Smith", e.DisplayName);
    }

    [TestMethod]
    public void SpanParse_RawAddress_Succeeds()
    {
        var e = EmailAddress.Parse("MiXeD@Example.Com".AsSpan(), null);
        Assert.AreEqual("mixed@example.com", e.Address);
    }

    [TestMethod]
    public void SpanParse_Combined_NormalizesLikeStringOverload()
    {
        var e = EmailAddress.Parse("JANE DOE <Jane.Doe@Example.com>".AsSpan(), CultureInfo.InvariantCulture);
        Assert.AreEqual("jane.doe@example.com", e.Address);
        Assert.AreEqual("Jane Doe", e.DisplayName);
    }

    [TestMethod]
    public void SpanParse_Invalid_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => EmailAddress.Parse("   ".AsSpan(), null));
        Assert.ThrowsExactly<FormatException>(() => EmailAddress.Parse("invalid@@example.com".AsSpan(), null));
    }

    [TestMethod]
    public void SpanTryParse_Combined_Succeeds()
    {
        Assert.IsTrue(EmailAddress.TryParse("John Smith <John.SMITH@Example.com>".AsSpan(),
            CultureInfo.InvariantCulture, out var e));
        Assert.AreEqual("john.smith@example.com", e.Address);
        Assert.AreEqual("John Smith", e.DisplayName);
    }

    [TestMethod]
    public void SpanTryParse_Invalid_ReturnsFalse()
    {
        Assert.IsFalse(EmailAddress.TryParse("   ".AsSpan(), null, out _));
        Assert.IsFalse(EmailAddress.TryParse("invalid@@example.com".AsSpan(), null, out _));
    }

    [TestMethod]
    public void ComparisonOperators_ConsistentWithCompareTo_ByAddress()
    {
        var a = EmailAddress.From("a@example.com");
        var b = EmailAddress.From("b@example.com");

        Assert.IsTrue(a < b);
        Assert.IsTrue(a <= b);
        Assert.IsFalse(a > b);
        Assert.IsFalse(a >= b);
        Assert.IsTrue(b > a);
        Assert.IsTrue(b >= a);
    }

    [TestMethod]
    public void ComparisonOperators_EqualAddresses_OnlyInclusiveHold()
    {
        var a = EmailAddress.From("same@example.com", "Alpha");
        var b = EmailAddress.From("SAME@EXAMPLE.COM", "Alpha");

        Assert.IsFalse(a < b);
        Assert.IsTrue(a <= b);
        Assert.IsFalse(a > b);
        Assert.IsTrue(a >= b);
    }
}