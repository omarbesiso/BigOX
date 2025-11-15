using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class StreamExtensionsTests
{
    [TestMethod]
    public void ToByteArray_NullStream_ThrowsArgumentNullException()
    {
        Stream? s = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => s!.ToByteArray());
    }

    [TestMethod]
    public void ToByteArray_MemoryStream_UsesToArray_DoesNotChangePosition()
    {
        var bytes = "hello"u8.ToArray();
        using var ms = new MemoryStream(bytes);
        ms.Position = 2; // arbitrary

        var result = ms.ToByteArray();

        CollectionAssert.AreEqual(bytes, result);
        Assert.AreEqual(2L, ms.Position, "MemoryStream fast-path should not alter Position");
    }

    [TestMethod]
    public void ToByteArray_SeekableNonMemoryStream_ReadsFromStartAndMovesToEnd()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var data = Enumerable.Range(0, 32).Select(i => (byte)i).ToArray();
            File.WriteAllBytes(tempPath, data);

            using var fs = File.OpenRead(tempPath);
            fs.Position = 5; // position should be ignored and reset to 0 by implementation

            var result = fs.ToByteArray();

            CollectionAssert.AreEqual(data, result);
            Assert.AreEqual(fs.Length, fs.Position, "Seekable stream should end at EOF after full read");
        }
        finally
        {
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
                /* ignore */
            }
        }
    }

    [TestMethod]
    public void ToByteArray_NonSeekableStream_ReadsFromCurrentPosition()
    {
        var all = "abcdef"u8.ToArray();
        using var inner = new MemoryStream(all);
        inner.Position = 2; // start from 'c'

        using var s = new NonSeekableReadStream(inner);
        var result = s.ToByteArray();

        CollectionAssert.AreEqual("cdef"u8.ToArray(), result);
    }

    [TestMethod]
    public async Task ToByteArrayAsync_MemoryStream_ReturnsCopy()
    {
        var bytes = new byte[] { 1, 2, 3, 4 };
        await using var ms = new MemoryStream(bytes);
        var result = await ms.ToByteArrayAsync();
        CollectionAssert.AreEqual(bytes, result);
    }

    [TestMethod]
    public async Task ToByteArrayAsync_SeekableNonMemoryStream_ReadsFromStartAndMovesToEnd()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var data = Enumerable.Range(0, 64).Select(i => (byte)(255 - i)).ToArray();
            await File.WriteAllBytesAsync(tempPath, data);

            await using var fs = File.OpenRead(tempPath);
            fs.Position = 10;

            var result = await fs.ToByteArrayAsync();

            CollectionAssert.AreEqual(data, result);
            Assert.AreEqual(fs.Length, fs.Position);
        }
        finally
        {
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
                /* ignore */
            }
        }
    }

    [TestMethod]
    public async Task ToByteArrayAsync_NonSeekableStream_ReadsFromCurrentPosition()
    {
        var bytes = "0123456789"u8.ToArray();
        await using var inner = new MemoryStream(bytes);
        inner.Position = 4; // start at '4'
        await using var s = new NonSeekableReadStream(inner);

        var result = await s.ToByteArrayAsync();
        CollectionAssert.AreEqual("456789"u8.ToArray(), result);
    }

    [TestMethod]
    public void ToByteArray_SeekableStream_LengthGreaterThanArrayMax_ThrowsArgumentException()
    {
        using var huge = new FakeSeekableHugeLengthStream(Array.MaxLength + 1L);
        Assert.ThrowsExactly<ArgumentException>(huge.ToByteArray);
    }

    [TestMethod]
    public async Task ToByteArrayAsync_NonSeekableStream_CanceledToken_ThrowsOperationCanceled()
    {
        await using var inner = new MemoryStream(new byte[1024]);
        await using var s = new NonSeekableReadStream(inner);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        try
        {
            _ = await s.ToByteArrayAsync(cts.Token);
            Assert.Fail("Expected OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            // expected
        }
    }

    private sealed class NonSeekableReadStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => inner.Position;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
            inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override ValueTask DisposeAsync()
        {
            return inner.DisposeAsync();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return inner.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return inner.CopyToAsync(destination, bufferSize, cancellationToken);
        }
    }

    private sealed class FakeSeekableHugeLengthStream(long length) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length { get; } = length;

        public override long Position { get; set; }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return Position = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => Position
            };
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}