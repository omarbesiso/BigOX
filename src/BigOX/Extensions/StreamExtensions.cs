using System.Buffers;

namespace BigOX.Extensions;

/// <summary>
///     Provides extension methods for working with <see cref="Stream" /> objects.
/// </summary>
public static class StreamExtensions
{
    private const int DefaultCopyBufferSize = 81920; // 80 KB - aligns with Stream.CopyTo default

    /// <summary>
    ///     Provides extension methods for the <see cref="Stream" /> class.
    /// </summary>
    /// <param name="stream">The <see cref="Stream" /> to convert to a byte array.</param>
    extension(Stream stream)
    {
        /// <summary>
        ///     Reads all bytes from the current stream and returns them as a byte array.
        /// </summary>
        /// <returns>
        ///     A byte array containing the contents of the stream from its current position
        ///     (or from the beginning for seekable streams).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="stream" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the stream length exceeds <see cref="Array.MaxLength" /> (approximately 2GB).
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the stream does not support reading.
        /// </exception>
        /// <exception cref="IOException">
        ///     Thrown when an I/O error occurs.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         <strong>Position Semantics:</strong>
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             For <strong>seekable</strong> streams: the stream is reset to position 0
        ///             before reading. The position is NOT restored afterward.
        ///         </item>
        ///         <item>
        ///             For <strong>non-seekable</strong> streams: reads from the current position
        ///             until the end of the stream.
        ///         </item>
        ///     </list>
        ///     <para>
        ///         <strong>Performance Characteristics:</strong>
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///             <strong>MemoryStream</strong>: O(n) - single allocation and copy via
        ///             <see cref="MemoryStream.ToArray" />.
        ///         </item>
        ///         <item>
        ///             <strong>Seekable streams with known length</strong>: O(1) allocation,
        ///             O(n) read using <see cref="Stream.ReadExactly(Span{byte})" />.
        ///         </item>
        ///         <item>
        ///             <strong>Non-seekable streams</strong>: O(log n) allocations (geometric growth),
        ///             O(n) total reads. Uses internal <see cref="MemoryStream" /> buffer.
        ///         </item>
        ///     </list>
        ///     <para>
        ///         <strong>Memory Allocation:</strong>
        ///     </para>
        ///     <para>
        ///         Arrays ≥ 85,000 bytes are allocated on the Large Object Heap (LOH). For high-throughput
        ///         scenarios with large streams, consider using <see cref="ArrayPool{T}" />-based alternatives
        ///         or streaming APIs to minimize LOH pressure.
        ///     </para>
        ///     <para>
        ///         <strong>Thread Safety:</strong>
        ///     </para>
        ///     <para>
        ///         This method is NOT thread-safe. The stream must not be accessed by other threads
        ///         during the call.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// using var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello, World!"));
        /// byte[] byteArray = stream.ToByteArray();
        /// // byteArray contains UTF-8 encoded bytes
        /// Console.WriteLine(byteArray.Length); // 13
        /// ]]></code>
        /// </example>
        public byte[] ToByteArray()
        {
            ArgumentNullException.ThrowIfNull(stream);

            // Fast path for MemoryStream - direct ToArray() call
            if (stream is MemoryStream sourceMemoryStream)
            {
                return sourceMemoryStream.ToArray();
            }

            // Seekable streams with known length
            if (stream.CanSeek)
            {
                var length = stream.Length;

                // Guard against arrays exceeding int.MaxValue
                if (length > Array.MaxLength)
                {
                    throw new ArgumentException(
                        $"Stream length ({length:N0} bytes) exceeds maximum array length ({Array.MaxLength:N0} bytes). " +
                        "Consider using streaming APIs or reading in chunks.",
                        nameof(stream));
                }

                // Reset to start and allocate exact-size buffer
                stream.Position = 0;
                var buffer = new byte[length];

                // ReadExactly guarantees full read or throws EndOfStreamException
                // Source: https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.readexactly
                stream.ReadExactly(buffer);

                return buffer;
            }

            // Non-seekable streams: use MemoryStream accumulator
            using var targetMemoryStream = new MemoryStream();
            stream.CopyTo(targetMemoryStream, DefaultCopyBufferSize);
            return targetMemoryStream.ToArray();
        }

        /// <summary>
        ///     Asynchronously reads all bytes from the current stream and returns them as a byte array.
        /// </summary>
        /// <param name="cancellationToken">
        ///     A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     a byte array with the stream contents.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="stream" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when the stream length exceeds <see cref="Array.MaxLength" />.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        ///     Thrown when the operation is canceled via <paramref name="cancellationToken" />.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown when the stream does not support reading.
        /// </exception>
        /// <exception cref="IOException">
        ///     Thrown when an I/O error occurs.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         This method uses <c>ConfigureAwait(false)</c> internally to avoid capturing
        ///         the synchronization context, making it suitable for library code.
        ///     </para>
        ///     <para>
        ///         See <see cref="ToByteArray" /> for detailed position semantics, performance
        ///         characteristics, and memory allocation notes.
        ///     </para>
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// await using var fileStream = File.OpenRead("data.bin");
        /// byte[] content = await fileStream.ToByteArrayAsync(cancellationToken);
        /// ]]></code>
        /// </example>
        public async Task<byte[]> ToByteArrayAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(stream);

            // Fast path for MemoryStream - synchronous ToArray() is fine
            if (stream is MemoryStream sourceMemoryStream)
            {
                // MemoryStream.ToArray() is a synchronous memory copy - no I/O involved
                return sourceMemoryStream.ToArray();
            }

            // Seekable streams with known length
            if (stream.CanSeek)
            {
                var length = stream.Length;

                // Guard against oversized arrays
                if (length > Array.MaxLength)
                {
                    throw new ArgumentException(
                        $"Stream length ({length:N0} bytes) exceeds maximum array length ({Array.MaxLength:N0} bytes). " +
                        "Consider using streaming APIs or reading in chunks.",
                        nameof(stream));
                }

                // Reset to start and allocate exact-size buffer
                stream.Position = 0;
                var buffer = new byte[length];

                // ReadExactlyAsync guarantees full read or throws EndOfStreamException
                // Source: https://learn.microsoft.com/en-us/dotnet/api/system.io.stream.readexactlyasync
                await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);

                return buffer;
            }

            // Non-seekable streams: use MemoryStream accumulator
            using var targetMemoryStream = new MemoryStream();
            await stream.CopyToAsync(targetMemoryStream, DefaultCopyBufferSize, cancellationToken)
                .ConfigureAwait(false);

            return targetMemoryStream.ToArray();
        }
    }
}