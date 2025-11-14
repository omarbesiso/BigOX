using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class ArrayExtensionsTests
{
    [TestMethod]
    public void ClearRange_ClearsExpectedSegment_ForValueTypes()
    {
        var arr = new[] { 1, 2, 3, 4, 5 };

        arr.ClearRange(1, 3);

        CollectionAssert.AreEqual((int[])[1, 0, 0, 0, 5], arr);
    }

    [TestMethod]
    public void ClearRange_ClearsExpectedSegment_ForReferenceTypes()
    {
        string?[] arr = ["a", "b", "c", "d"];

        arr.ClearRange(1, 2);

        CollectionAssert.AreEqual(new[] { "a", null, null, "d" }, arr);
    }

    [TestMethod]
    public void ClearRange_LengthZero_NoChange()
    {
        var arr = new[] { 1, 2, 3 };

        arr.ClearRange(1, 0);

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, arr);
    }

    [TestMethod]
    public void ClearRange_IndexZero_ClearAll()
    {
        var arr = new[] { 1, 2, 3 };

        arr.ClearRange(0, 3);

        CollectionAssert.AreEqual(new[] { 0, 0, 0 }, arr);
    }

    [TestMethod]
    public void ClearRange_NullArray_ThrowsArgumentNullException()
    {
        int[]? arr = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => arr!.ClearRange(0, 1));
    }

    [TestMethod]
    public void ClearRange_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        var arr = new[] { 1, 2, 3 };

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => arr.ClearRange(-1, 1));
    }

    [TestMethod]
    public void ClearRange_NegativeLength_ThrowsArgumentOutOfRangeException()
    {
        var arr = new[] { 1, 2, 3 };

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => arr.ClearRange(0, -1));
    }

    [TestMethod]
    public void ClearRange_IndexPlusLengthBeyondArray_ThrowsArgumentOutOfRangeException()
    {
        var arr = new[] { 1, 2, 3 };

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => arr.ClearRange(2, 2));
    }

    [TestMethod]
    public void Clear_WithRange_ClearsExpectedSlice()
    {
        var arr = new[] { 1, 2, 3, 4, 5 };

        arr.Clear(1..4);

        CollectionAssert.AreEqual((int[])[1, 0, 0, 0, 5], arr);
    }

    [TestMethod]
    public void Clear_WithRange_FullRange()
    {
        var arr = new[] { 1, 2, 3 };

        arr.Clear(..);

        CollectionAssert.AreEqual((int[])[0, 0, 0], arr);
    }

    [TestMethod]
    public void Clear_WithRange_NullArray_ThrowsArgumentNullException()
    {
        int[]? arr = null;

        Assert.ThrowsExactly<ArgumentNullException>(() => arr!.Clear(1..2));
    }

    [TestMethod]
    public void Clear_WithRange_InvalidRange_ThrowsArgumentOutOfRangeException_WhenEndLessThanStart()
    {
        var arr = new[] { 1, 2, 3, 4, 5 };

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => arr.Clear(4..3));
    }

    [TestMethod]
    public void Clear_WithRange_EndBeyondLength_ThrowsArgumentOutOfRangeException()
    {
        var arr = new[] { 1, 2, 3, 4, 5 };

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => arr.Clear(3..10));
    }
}