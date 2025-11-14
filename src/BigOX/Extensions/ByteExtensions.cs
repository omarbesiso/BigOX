using System.Runtime.CompilerServices;

namespace BigOX.Extensions;

/// <summary>
///     Extension helpers for creating <see cref="MemoryStream" /> instances from byte data.
/// </summary>
/// <remarks>
///     <para>
///         Overloads that accept a <see cref="byte" /> array create a non-resizable <see cref="MemoryStream" />
///         that directly references the provided array (zero-copy). Changes made via the stream are reflected in the
///         original array, and vice versa.
///     </para>
///     <para>
///         The <see cref="ReadOnlyMemory{T}" /> overload allocates a new array via
///         <see cref="ReadOnlyMemory{T}.ToArray" />
///         and constructs the stream over that copy (no shared backing storage).
///     </para>
/// </remarks>
public static class ByteExtensions
{
    /// <param name="readOnlyMemory">
    ///     The read-only byte memory to convert.
    /// </param>
    extension(ReadOnlyMemory<byte> readOnlyMemory)
    {
        /// <summary>
        ///     Creates a <see cref="MemoryStream" /> from a <see cref="ReadOnlyMemory{T}" /> of <see cref="byte" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="MemoryStream" /> containing a copy of the data in the input.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         This overload allocates a new array via <see cref="ReadOnlyMemory{T}.ToArray" /> and constructs the
        ///         stream over that copy. The stream is independent of the original <see cref="ReadOnlyMemory{T}" />.
        ///     </para>
        ///     <para>
        ///         The resulting stream is non-resizable and is initially positioned at <c>0</c>.
        ///     </para>
        /// </remarks>
        /// <seealso cref="MemoryStream" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryStream ToMemoryStream()
        {
            return new MemoryStream(readOnlyMemory.ToArray());
        }
    }

    /// <param name="buffer">
    ///     The source <see cref="byte" /> array from which the <see cref="MemoryStream" /> is created.
    ///     When used, the resulting stream is non-resizable and references this array directly.
    /// </param>
    extension(byte[] buffer)
    {
        /// <summary>
        ///     Creates a non-resizable <see cref="MemoryStream" /> over the entire <paramref name="buffer" />.
        /// </summary>
        /// <param name="writable">
        ///     If <c>true</c>, the stream supports writing and mutations are reflected in <paramref name="buffer" />.
        ///     If <c>false</c>, write operations throw <see cref="NotSupportedException" />.
        ///     Defaults to <c>false</c>.
        /// </param>
        /// <returns>
        ///     A <see cref="MemoryStream" /> whose <see cref="MemoryStream.Length" /> and
        ///     <see cref="MemoryStream.Capacity" /> equal <c>buffer.Length</c>, with
        ///     <see cref="MemoryStream.Position" /> set to <c>0</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="buffer" /> is <c>null</c>.
        /// </exception>
        /// <remarks>
        ///     This method avoids copying by constructing the stream directly over the provided array.
        ///     The stream cannot grow beyond the original array length.
        /// </remarks>
        /// <seealso cref="MemoryStream" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryStream ToMemoryStream(bool writable = false)
        {
            return new MemoryStream(buffer, writable);
        }

        /// <summary>
        ///     Creates a non-resizable <see cref="MemoryStream" /> over a subrange of <paramref name="buffer" />.
        /// </summary>
        /// <param name="index">
        ///     The zero-based offset into <paramref name="buffer" /> at which the stream begins.
        /// </param>
        /// <param name="count">
        ///     The number of bytes from <paramref name="buffer" /> to expose via the stream.
        /// </param>
        /// <param name="writable">
        ///     If <c>true</c>, the stream supports writing within the specified slice and mutations are reflected
        ///     in <paramref name="buffer" />. If <c>false</c>, write operations throw <see cref="NotSupportedException" />.
        ///     Defaults to <c>false</c>.
        /// </param>
        /// <returns>
        ///     A <see cref="MemoryStream" /> over the range <c>[index, index + count)</c> of <paramref name="buffer" />,
        ///     with <see cref="MemoryStream.Length" /> equal to <paramref name="count" /> and
        ///     <see cref="MemoryStream.Position" /> set to <c>0</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if <paramref name="buffer" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="index" /> or <paramref name="count" /> is less than <c>0</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown if <paramref name="index" /> and <paramref name="count" /> do not denote a valid range within
        ///     <paramref name="buffer" /> (i.e., if <c>index + count &gt; buffer.Length</c>).
        /// </exception>
        /// <remarks>
        ///     This method avoids copying by constructing the stream directly over the specified slice of the array.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     byte[] data = { 0x1, 0x2, 0x3, 0x4, 0x5 };
        ///     using (MemoryStream stream = data.ToMemoryStream(1, 3))
        ///     {
        ///         // stream contains { 0x2, 0x3, 0x4 }
        ///     }
        ///     ]]></code>
        /// </example>
        /// <seealso cref="MemoryStream" />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MemoryStream ToMemoryStream(int index, int count, bool writable = false)
        {
            return new MemoryStream(buffer, index, count, writable);
        }
    }
}