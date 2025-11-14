using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class ByteExtensionsTests
{
    [TestMethod]
    public void ToMemoryStream_ByteArray_DefaultReadOnly_ReflectsArrayChanges()
    {
        byte[] data = [1, 2, 3, 4];

#pragma warning disable IDE0063
        using (var stream = data.ToMemoryStream())
#pragma warning restore IDE0063
        {
            Assert.IsTrue(stream.CanRead);
            Assert.IsFalse(stream.CanWrite);
            Assert.AreEqual(data.Length, stream.Length);
            Assert.AreEqual(0L, stream.Position);

            // Mutate the source array and ensure the stream reflects the change (zero-copy behavior)
            data[1] = 9;
            stream.Position = 0;
            var snapshot = stream.ToArray();
            CollectionAssert.AreEqual((byte[])[1, 9, 3, 4], snapshot);

            // Verify writes are not allowed when writable == false
            Assert.ThrowsExactly<NotSupportedException>(() => stream.WriteByte(0xFF));
        }
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_WritableTrue_WritesReflectInArray()
    {
        byte[] data = [10, 20, 30, 40];

        using var stream = data.ToMemoryStream(true);

        Assert.IsTrue(stream.CanWrite);

        // Write within the stream and ensure the backing array is updated
        stream.Position = 2;
        stream.WriteByte(0xEE);
        CollectionAssert.AreEqual((byte[])[10, 20, 0xEE, 40], data);
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_Slice_DefaultProperties()
    {
        byte[] data = [1, 2, 3, 4, 5];

        using var stream = data.ToMemoryStream(1, 3);

        Assert.AreEqual(3L, stream.Length);
        Assert.AreEqual(0L, stream.Position);
        Assert.IsTrue(stream.CanRead);
        Assert.IsFalse(stream.CanWrite);

        // Reading the slice should yield {2,3,4}
        var slice = stream.ToArray();
        CollectionAssert.AreEqual((byte[])[2, 3, 4], slice);

        // Mutate the original array within the slice and ensure stream reflects it
        data[2] = 42; // corresponds to slice index 1
        stream.Position = 0;
        var mutated = stream.ToArray();
        CollectionAssert.AreEqual((byte[])[2, 42, 4], mutated);
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_Slice_Writable_MutatesUnderlyingArray()
    {
        var data = "defgh"u8.ToArray();

        using var stream = data.ToMemoryStream(1, 3, true);

        // Write at relative slice position 2 -> absolute array index 3
        stream.Position = 2;
        stream.WriteByte(0xAA);

        CollectionAssert.AreEqual((byte[])[100, 101, 102, 0xAA, 104], data);
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_Null_ThrowsArgumentNullException()
    {
        byte[]? data = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => data!.ToMemoryStream());
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_Slice_InvalidRange_ThrowsArgumentException()
    {
        byte[] data = [1, 2, 3];
        Assert.ThrowsExactly<ArgumentException>(() => data.ToMemoryStream(2, 2));
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_Slice_NegativeIndex_ThrowsArgumentOutOfRangeException()
    {
        byte[] data = [1, 2, 3];
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => data.ToMemoryStream(-1, 1));
    }

    [TestMethod]
    public void ToMemoryStream_ByteArray_Slice_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        byte[] data = [1, 2, 3];
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => data.ToMemoryStream(0, -1));
    }

    [TestMethod]
    public void ToMemoryStream_ReadOnlyMemory_CreatesIndependentCopy()
    {
        var backing = new byte[] { 1, 2, 3, 4 };
        ReadOnlyMemory<byte> rom = backing;

        using var stream = rom.ToMemoryStream();

        // Mutate the original backing array AFTER creating the stream; stream should be unaffected
        backing[1] = 9;

        stream.Position = 0;
        var fromStream = stream.ToArray();
        CollectionAssert.AreEqual((byte[])[1, 2, 3, 4], fromStream);
    }

    [TestMethod]
    public void ToMemoryStream_ReadOnlyMemory_StreamWritableWithinBounds_DoesNotAffectSource()
    {
        var backing = new byte[] { 5, 6, 7 };
        ReadOnlyMemory<byte> rom = backing;

        using var stream = rom.ToMemoryStream();

        Assert.IsTrue(stream.CanWrite);

        stream.Position = 1;
        stream.WriteByte(0xFF);

        // Original backing array remains unchanged (copy semantics)
        CollectionAssert.AreEqual(new byte[] { 5, 6, 7 }, backing);

        // Stream data has changed
        stream.Position = 0;
        var fromStream = stream.ToArray();
        CollectionAssert.AreEqual((byte[])[5, 0xFF, 7], fromStream);
    }
}