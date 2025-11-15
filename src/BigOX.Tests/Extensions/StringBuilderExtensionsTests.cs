using System.Text;
using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class StringBuilderExtensionsTests
{
    [TestMethod]
    public void IsEmpty_EmptyBuilder_ReturnsTrue()
    {
        var sb = new StringBuilder();
        Assert.IsTrue(sb.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_WhitespaceOnly_CountWhitespaceTrue_ReturnsTrue()
    {
        var sb = new StringBuilder("   \t\n");
        Assert.IsTrue(sb.IsEmpty(true));
        Assert.IsFalse(sb.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_NonWhitespace_CountWhitespaceTrue_ReturnsFalse()
    {
        var sb = new StringBuilder(" a ");
        Assert.IsFalse(sb.IsEmpty(true));
    }

    [TestMethod]
    public void IsEmpty_NullBuilder_ThrowsArgumentNullException()
    {
        StringBuilder? sb = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => sb!.IsEmpty());
    }

    [TestMethod]
    public void AppendCharToLength_AppendsUntilTargetLength()
    {
        var sb = new StringBuilder("ab");
        sb.AppendCharToLength(5, 'x');
        Assert.AreEqual("abxxx", sb.ToString());
    }

    [TestMethod]
    public void AppendCharToLength_TargetLengthLessOrEqual_NoChange()
    {
        var sb = new StringBuilder("abcdef");
        sb.AppendCharToLength(3, '.');
        Assert.AreEqual("abcdef", sb.ToString());
    }

    [TestMethod]
    public void AppendCharToLength_NegativeTarget_ThrowsArgumentOutOfRangeException()
    {
        var sb = new StringBuilder("a");
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sb.AppendCharToLength(-1, 'x'));
    }

    [TestMethod]
    public void ReduceToLength_Truncates()
    {
        var sb = new StringBuilder("abcdef");
        sb.ReduceToLength(3);
        Assert.AreEqual("abc", sb.ToString());
    }

    [TestMethod]
    public void ReduceToLength_MaxLengthZero_Clears()
    {
        var sb = new StringBuilder("abc");
        sb.ReduceToLength(0);
        Assert.AreEqual(string.Empty, sb.ToString());
    }

    [TestMethod]
    public void ReduceToLength_MaxLengthGreater_NoChange()
    {
        var sb = new StringBuilder("abc");
        sb.ReduceToLength(10);
        Assert.AreEqual("abc", sb.ToString());
    }

    [TestMethod]
    public void ReduceToLength_NegativeTarget_ThrowsArgumentOutOfRangeException()
    {
        var sb = new StringBuilder("abc");
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sb.ReduceToLength(-1));
    }

    [TestMethod]
    public void Reverse_ReversesString()
    {
        var sb = new StringBuilder("abcd");
        sb.Reverse();
        Assert.AreEqual("dcba", sb.ToString());
    }

    [TestMethod]
    public void Reverse_SingleChar_NoChange()
    {
        var sb = new StringBuilder("x");
        sb.Reverse();
        Assert.AreEqual("x", sb.ToString());
    }

    [TestMethod]
    public void EnsureStartsWith_AddsPrefix_WhenMissing()
    {
        var sb = new StringBuilder("Value");
        sb.EnsureStartsWith("Pre");
        Assert.AreEqual("PreValue", sb.ToString());
    }

    [TestMethod]
    public void EnsureStartsWith_AlreadyHasPrefix_NoChange()
    {
        var sb = new StringBuilder("TestValue");
        sb.EnsureStartsWith("Test");
        Assert.AreEqual("TestValue", sb.ToString());
    }

    [TestMethod]
    public void EnsureStartsWith_CaseInsensitive_NoDuplicate()
    {
        var sb = new StringBuilder("testValue");
        sb.EnsureStartsWith("Test", StringComparison.OrdinalIgnoreCase);
        Assert.AreEqual("testValue", sb.ToString());
    }

    [TestMethod]
    public void EnsureStartsWith_NullOrEmpty_Throws()
    {
        var sb = new StringBuilder("abc");
        Assert.ThrowsExactly<ArgumentNullException>(() => sb.EnsureStartsWith(null!));
        Assert.ThrowsExactly<ArgumentException>(() => sb.EnsureStartsWith(""));
    }

    [TestMethod]
    public void EnsureEndsWith_AddsSuffix_WhenMissing()
    {
        var sb = new StringBuilder("Value");
        sb.EnsureEndsWith("End");
        Assert.AreEqual("ValueEnd", sb.ToString());
    }

    [TestMethod]
    public void EnsureEndsWith_AlreadyHasSuffix_NoChange()
    {
        var sb = new StringBuilder("ValueEnd");
        sb.EnsureEndsWith("End");
        Assert.AreEqual("ValueEnd", sb.ToString());
    }

    [TestMethod]
    public void EnsureEndsWith_CaseInsensitive_NoDuplicate()
    {
        var sb = new StringBuilder("Valueend");
        sb.EnsureEndsWith("End", StringComparison.OrdinalIgnoreCase);
        Assert.AreEqual("Valueend", sb.ToString());
    }

    [TestMethod]
    public void EnsureEndsWith_NullOrEmpty_Throws()
    {
        var sb = new StringBuilder("abc");
        Assert.ThrowsExactly<ArgumentNullException>(() => sb.EnsureEndsWith(null!));
        Assert.ThrowsExactly<ArgumentException>(() => sb.EnsureEndsWith(""));
    }

    [TestMethod]
    public void AppendMultiple_AppendsItems_NoTrailingNewLine()
    {
        var sb = new StringBuilder();
        // ReSharper disable once RedundantExplicitParamsArrayCreation
        sb.AppendMultiple(true, ["A", null, "B", "", "C"]);
        Assert.AreEqual($"A{Environment.NewLine}B{Environment.NewLine}C", sb.ToString());
    }

    [TestMethod]
    public void AppendMultiple_NoItems_Ignores()
    {
        var sb = new StringBuilder();
        sb.AppendMultiple(true); // no items
        Assert.AreEqual(string.Empty, sb.ToString());
    }

    [TestMethod]
    public void RemoveAllOccurrences_RemovesSpecifiedChar()
    {
        var sb = new StringBuilder("a_b_c_");
        sb.RemoveAllOccurrences('_');
        Assert.AreEqual("abc", sb.ToString());
    }

    [TestMethod]
    public void RemoveAllOccurrences_NonePresent_NoChange()
    {
        var sb = new StringBuilder("abc");
        sb.RemoveAllOccurrences('_');
        Assert.AreEqual("abc", sb.ToString());
    }

    [TestMethod]
    public void RemoveAllOccurrences_AllPresent_ResultsEmpty()
    {
        var sb = new StringBuilder("aaa");
        sb.RemoveAllOccurrences('a');
        Assert.AreEqual(string.Empty, sb.ToString());
    }

    [TestMethod]
    public void Trim_TrimsLeadingAndTrailingWhitespace()
    {
        var sb = new StringBuilder("  abc  ");
        sb.Trim();
        Assert.AreEqual("abc", sb.ToString());
    }

    [TestMethod]
    public void Trim_AllWhitespace_Clears()
    {
        var sb = new StringBuilder("   \t\n ");
        sb.Trim();
        Assert.AreEqual(string.Empty, sb.ToString());
    }

    [TestMethod]
    public void Trim_NoWhitespace_NoChange()
    {
        var sb = new StringBuilder("abc");
        sb.Trim();
        Assert.AreEqual("abc", sb.ToString());
    }

    [TestMethod]
    public void AppendFormatLine_WithItems_AppendsFormattedLine()
    {
        var sb = new StringBuilder();
        sb.AppendFormatLine("Value: {0}", 42);
        Assert.AreEqual($"Value: 42{Environment.NewLine}", sb.ToString());
    }

    [TestMethod]
    public void AppendFormatLine_NoItems_AppendsFormatVerbatim()
    {
        var sb = new StringBuilder();
        sb.AppendFormatLine("Hello");
        Assert.AreEqual($"Hello{Environment.NewLine}", sb.ToString());
    }

    [TestMethod]
    public void AppendFormatLine_NullOrWhitespaceFormat_Throws()
    {
        var sb = new StringBuilder();
        Assert.ThrowsExactly<ArgumentNullException>(() => sb.AppendFormatLine(null!));
        Assert.ThrowsExactly<ArgumentException>(() => sb.AppendFormatLine("   "));
    }

    [TestMethod]
    public void AppendMultipleLines_AppendsExpectedNumber()
    {
        var sb = new StringBuilder("Start");
        sb.AppendMultipleLines(3);
        Assert.AreEqual($"Start{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}", sb.ToString());
    }

    [TestMethod]
    public void AppendMultipleLines_InvalidCount_Throws()
    {
        var sb = new StringBuilder();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => sb.AppendMultipleLines(0));
    }
}