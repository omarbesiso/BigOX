using BigOX.Types;

namespace BigOX.Tests.Types;

[TestClass]
public sealed class DateRangeTests
{
    private const char InfinityChar = '\u221E';

    [TestMethod]
    public void DefaultValue_IsMinStartAndOpenEnded()
    {
        var r = default(DateRange);
        Assert.AreEqual(DateOnly.MinValue, r.StartDate);
        Assert.IsNull(r.EndDate);
        Assert.IsTrue(r.IsOpenEnded);
        Assert.AreEqual(DateOnly.MaxValue, r.EffectiveEnd);
        Assert.AreEqual($"0001-01-01|{InfinityChar}", r.ToString());
    }

    [TestMethod]
    public void Constructor_ClosedRange_AllowsSameStartEnd()
    {
        var d = new DateOnly(2024, 5, 17);
        var r = new DateRange(d, d);
        Assert.AreEqual(d, r.StartDate);
        Assert.AreEqual(d, r.EndDate);
        Assert.IsFalse(r.IsOpenEnded);
    }

    [TestMethod]
    public void Constructor_EndBeforeStart_ThrowsArgumentOutOfRangeException()
    {
        var start = new DateOnly(2024, 1, 2);
        var end = new DateOnly(2024, 1, 1);
        // The constructor currently throws ArgumentOutOfRangeException for invalid ordering; assert that exact type.
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new DateRange(start, end));
    }

    [TestMethod]
    public void Factory_Create_ReturnsEquivalentInstance()
    {
        var start = new DateOnly(2024, 1, 1);
        var end = new DateOnly(2024, 12, 31);
        var a = new DateRange(start, end);
        var b = DateRange.Create(start, end);
        Assert.AreEqual(a, b);
    }

    [TestMethod]
    public void OpenEnded_ToString_FormatsCanonical()
    {
        var start = new DateOnly(2024, 2, 1);
        var r = new DateRange(start);
        Assert.AreEqual($"2024-02-01|{InfinityChar}", r.ToString());
    }

    [TestMethod]
    public void Closed_ToString_FormatsCanonical()
    {
        var r = new DateRange(new DateOnly(2024, 2, 1), new DateOnly(2024, 2, 29));
        Assert.AreEqual("2024-02-01|2024-02-29", r.ToString());
    }

    [TestMethod]
    public void TryFormat_TooSmallDestination_ReturnsFalse()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 31));
        Span<char> dest = stackalloc char[5];
        var ok = r.TryFormat(dest, out var written, default, null);
        Assert.IsFalse(ok);
        Assert.AreEqual(0, written);
    }

    [TestMethod]
    public void TryFormat_OpenEnded_WritesExpectedLength()
    {
        var r = new DateRange(new DateOnly(2024, 1, 1));
        Span<char> dest = stackalloc char[32];
        var ok = r.TryFormat(dest, out var written, default, null);
        Assert.IsTrue(ok);
        var s = new string(dest[..written]);
        Assert.AreEqual($"2024-01-01|{InfinityChar}", s);
    }

    [TestMethod]
    public void TryParse_ClosedRange_Succeeds()
    {
        var input = "2024-02-01|2024-02-29"; // leap year day
        Assert.IsTrue(DateRange.TryParse(input, out var r));
        Assert.AreEqual(new DateOnly(2024, 2, 1), r.StartDate);
        Assert.AreEqual(new DateOnly(2024, 2, 29), r.EndDate);
        Assert.IsFalse(r.IsOpenEnded);
    }

    [TestMethod]
    public void TryParse_OpenEnded_Succeeds()
    {
        var input = $"2024-02-01|{InfinityChar}";
        Assert.IsTrue(DateRange.TryParse(input, out var r));
        Assert.AreEqual(new DateOnly(2024, 2, 1), r.StartDate);
        Assert.IsNull(r.EndDate);
        Assert.IsTrue(r.IsOpenEnded);
    }

    [TestMethod]
    public void TryParse_WithWhitespace_Succeeds()
    {
        var input = " 2024-02-01  |  2024-02-29  ";
        Assert.IsTrue(DateRange.TryParse(input, out var r));
        Assert.AreEqual("2024-02-01|2024-02-29", r.ToString());
    }

    [TestMethod]
    public void TryParse_SpanOverload_Succeeds()
    {
        var spanInput = "2024-03-01|2024-03-31".AsSpan();
        Assert.IsTrue(DateRange.TryParse(spanInput, out var r));
        Assert.AreEqual("2024-03-01|2024-03-31", r.ToString());
    }

    [TestMethod]
    public void TryParse_Invalid_EndBeforeStart_Fails()
    {
        Assert.IsFalse(DateRange.TryParse("2024-02-02|2024-02-01", out _));
    }

    [TestMethod]
    public void TryParse_Invalid_DoubleSeparator_Fails()
    {
        Assert.IsFalse(DateRange.TryParse("2024-02-01||2024-02-29", out _));
    }

    [TestMethod]
    public void TryParse_Invalid_MissingSeparator_Fails()
    {
        Assert.IsFalse(DateRange.TryParse("2024-02-012024-02-29", out _));
    }

    [TestMethod]
    public void Parse_Invalid_ThrowsFormatException()
    {
        Assert.ThrowsExactly<FormatException>(() => DateRange.Parse("not-a-range"));
    }

    [TestMethod]
    public void Parse_Span_Invalid_ThrowsFormatException()
    {
        // Avoid capturing ref span in lambda; call directly then assert.
        var spanInput = "bad".AsSpan();
        try
        {
            _ = DateRange.Parse(spanInput, null);
            Assert.Fail("Expected FormatException");
        }
        catch (FormatException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void Equality_OpenEnded_AreEqual()
    {
        var a = new DateRange(new DateOnly(2024, 1, 1));
        var b = new DateRange(new DateOnly(2024, 1, 1));
        Assert.IsTrue(a == b);
        Assert.AreEqual(a, b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void Equality_ClosedRanges_AreEqualAndHashCodesMatch()
    {
        var a = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));
        var b = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));
        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Inequality_DifferentStartOrEnd()
    {
        var a = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 31));
        var b = new DateRange(new DateOnly(2024, 1, 2), new DateOnly(2024, 12, 31));
        var c = new DateRange(new DateOnly(2024, 1, 1), new DateOnly(2024, 12, 30));
        Assert.IsTrue(a != b);
        Assert.IsTrue(a != c);
    }

    [TestMethod]
    public void Deconstruct_Works()
    {
        var r = new DateRange(new DateOnly(2024, 4, 1), new DateOnly(2024, 4, 30));
        var (s, e) = r;
        Assert.AreEqual(r.StartDate, s);
        Assert.AreEqual(r.EndDate, e);
    }
}