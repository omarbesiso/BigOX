using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="IEnumerable" /> objects.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    ///     Provides extension methods for <see cref="IEnumerable" />.
    /// </summary>
    /// <param name="collection">The collection to check for emptiness.</param>
    extension(IEnumerable collection)
    {
        /// <summary>
        ///     Determines whether the specified collection is empty.
        /// </summary>
        /// <returns>true if the specified collection is empty; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
        /// <remarks>
        ///     This extension method can be used on any type that implements the <see cref="System.Collections.IEnumerable" />
        ///     interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsEmpty" /> method to check if an array is empty.
        ///     <code><![CDATA[
        /// int[] emptyArray = new int[0];
        /// bool isEmpty = emptyArray.IsEmpty();
        /// Console.WriteLine(isEmpty); // Output: True
        /// ]]></code>
        /// </example>
        [Pure]
        public bool IsEmpty()
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(collection);

            switch (collection)
            {
                case Array a:
                    return a.Length == 0;
                case ICollection c:
                    return c.Count == 0;
                case IReadOnlyCollection<object> rc:
                    return rc.Count == 0;
                default:
                {
                    // ReSharper disable once PossibleMultipleEnumeration
                    var enumerator = collection.GetEnumerator();
                    try
                    {
                        return !enumerator.MoveNext();
                    }
                    finally
                    {
                        if (enumerator is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     Determines whether the specified collection is not empty.
        /// </summary>
        /// <returns>true if the specified collection is not empty; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
        /// <remarks>
        ///     This extension method can be used on any type that implements the <see cref="System.Collections.IEnumerable" />
        ///     interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsNotEmpty" /> method to check if an array is not empty.
        ///     <code><![CDATA[
        /// int[] nonEmptyArray = new int[] { 1, 2, 3 };
        /// bool isNotEmpty = nonEmptyArray.IsNotEmpty();
        /// Console.WriteLine(isNotEmpty); // Output: True
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty()
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(collection);
            // ReSharper disable once PossibleMultipleEnumeration
            return !collection.IsEmpty();
        }
    }

    /// <summary>
    ///     Provides extension methods for <see cref="IEnumerable{T}" />.
    /// </summary>
    /// <param name="collection">The collection to be split into chunks.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    extension<T>(IEnumerable<T> collection)
    {
        /// <summary>
        ///     Determines whether the specified collection is empty.
        /// </summary>
        /// <returns>true if the specified collection is empty; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
        /// <remarks>
        ///     This extension method can be used on any type that implements the
        ///     <see cref="System.Collections.Generic.IEnumerable{T}" /> interface, including arrays and lists. The collection is
        ///     not fully enumerated: when the element count is available without iterating (for example arrays and
        ///     <see cref="System.Collections.Generic.ICollection{T}" /> instances) it is used directly; otherwise at most one
        ///     element is consumed from the sequence to determine whether it is empty.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsEmpty{T}(IEnumerable{T})" /> method to check if a list
        ///     is empty.
        ///     <code><![CDATA[
        /// List<int> emptyList = new List<int>();
        /// bool isEmpty = emptyList.IsEmpty();
        /// Console.WriteLine(isEmpty); // Output: True
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty()
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(collection);

            // ReSharper disable once PossibleMultipleEnumeration
            if (collection.TryGetNonEnumeratedCount(out var count))
            {
                return count == 0;
            }

            // ReSharper disable once PossibleMultipleEnumeration
            using var enumerator = collection.GetEnumerator();
            return !enumerator.MoveNext();
        }

        /// <summary>
        ///     Determines whether the specified collection is not empty.
        /// </summary>
        /// <returns>true if the specified collection is not empty; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection is null.</exception>
        /// <remarks>
        ///     This extension method can be used on any type that implements the
        ///     <see cref="System.Collections.Generic.IEnumerable{T}" /> interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsNotEmpty(System.Collections.IEnumerable)" /> method to
        ///     check if a list is not empty.
        ///     <code><![CDATA[
        /// List<int> nonEmptyList = new List<int>() { 1, 2, 3 };
        /// bool isNotEmpty = nonEmptyList.IsNotEmpty();
        /// Console.WriteLine(isNotEmpty); // Output: True
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotEmpty()
        {
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(collection);
            // ReSharper disable once PossibleMultipleEnumeration
            return !collection.IsEmpty();
        }

        /// <summary>
        ///     Splits the input collection into smaller chunks of the specified size.
        /// </summary>
        /// <param name="chunkSize">The size of each chunk.</param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}" /> of chunks, where each chunk is an <see cref="IEnumerable{T}" /> of elements
        ///     from the input collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the collection is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="chunkSize" /> is less than or equal to 0.</exception>
        /// <example>
        ///     <code><![CDATA[
        /// var inputList = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        /// int chunkSize = 3;
        /// 
        /// var chunks = inputList.Chunk(chunkSize);
        /// 
        /// // The chunks variable now contains the following chunks: [1, 2, 3], [4, 5, 6], and [7, 8, 9].
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     <para>
        ///         This method divides a large collection into smaller chunks of the specified size. The last chunk may
        ///         contain fewer elements if the input collection's size is not evenly divisible by the specified chunk
        ///         size. The input collection remains unchanged.
        ///     </para>
        ///     <para>
        ///         <strong>Streaming contract:</strong> the returned chunks are lazy and all share the single underlying
        ///         enumerator of the source. Each chunk must be enumerated to completion, in order, before advancing to the
        ///         next chunk. Do <strong>not</strong> buffer the outer sequence (for example with <c>chunks.ToList()</c>),
        ///         and do not enumerate chunks out of order or concurrently: because they share one enumerator, doing so
        ///         silently corrupts or drops elements rather than throwing.
        ///     </para>
        ///     <para>
        ///         <c>System.Linq.Enumerable</c> exposes a method also named <c>Chunk</c>, so a caller importing both
        ///         <c>BigOX.Extensions</c> and <c>System.Linq</c> will see a CS0121 ambiguous-call error. Qualify the call
        ///         (for example <c>EnumerableExtensions.Chunk(source, size)</c>) to select this streaming overload.
        ///     </para>
        /// </remarks>
        [Pure]
        public IEnumerable<IEnumerable<T>> Chunk(int chunkSize)
        {
            // Validate parameters
            // ReSharper disable once PossibleMultipleEnumeration
            Guard.NotNull(collection);

            if (chunkSize <= 0)
            {
                throw new ArgumentException($"The chunk size must be greater than 0. Provided value: {chunkSize}",
                    nameof(chunkSize));
            }

            // Use an enumerator to yield chunks
            // ReSharper disable once PossibleMultipleEnumeration
            using var enumerator = collection.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return GetChunk(enumerator, chunkSize);
            }

            yield break;

            // Local static method to create a chunk
            static IEnumerable<TK> GetChunk<TK>(IEnumerator<TK> enumerator, int chunkSize)
            {
                do
                {
                    yield return enumerator.Current;
                } while (--chunkSize > 0 && enumerator.MoveNext());
            }
        }
    }

    /// <summary>
    ///     Provides extension methods for <see cref="IEnumerable" />.
    /// </summary>
    /// <param name="collection">The collection to check for null or emptiness.</param>
    extension([NotNullWhen(false)] IEnumerable? collection)
    {
        /// <summary>
        ///     Determines whether the specified collection is null, empty, or contains no elements.
        /// </summary>
        /// <returns>
        ///     true if the specified collection is null, empty, or contains no elements; otherwise,
        ///     false.
        /// </returns>
        /// <remarks>
        ///     This extension method can be used on any type that implements the <see cref="System.Collections.IEnumerable" />
        ///     interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsNullOrEmpty" /> method to check if an array is null or
        ///     empty.
        ///     <code><![CDATA[
        /// int[] emptyArray = new int[0];
        /// int[] nullArray = null;
        /// bool isNullOrEmpty1 = emptyArray.IsNullOrEmpty();
        /// bool isNullOrEmpty2 = nullArray.IsNullOrEmpty();
        /// Console.WriteLine(isNullOrEmpty1); // Output: True
        /// Console.WriteLine(isNullOrEmpty2); // Output: True
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullOrEmpty() => collection == null || collection.IsEmpty();
    }

    /// <param name="collection">The collection to check for null or emptiness.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    extension<T>([NotNullWhen(false)] IEnumerable<T>? collection)
    {
        /// <summary>
        ///     Determines whether the specified collection is null, empty, or contains no elements.
        /// </summary>
        /// <returns>
        ///     true if the specified collection is null, empty, or contains no elements; otherwise,
        ///     false.
        /// </returns>
        /// <remarks>
        ///     This extension method can be used on any type that implements the
        ///     <see cref="System.Collections.Generic.IEnumerable{T}" /> interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsNullOrEmpty{T}(IEnumerable{T})" /> method to check if a
        ///     list is null or empty.
        ///     <code><![CDATA[
        /// List<int> emptyList = new List<int>();
        /// List<int> nullList = null;
        /// bool isNullOrEmpty1 = emptyList.IsNullOrEmpty();
        /// bool isNullOrEmpty2 = nullList.IsNullOrEmpty();
        /// Console.WriteLine(isNullOrEmpty1); // Output: True
        /// Console.WriteLine(isNullOrEmpty2); // Output: True
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNullOrEmpty() => collection == null || collection.IsEmpty();
    }

    /// <param name="collection">The collection to check for non-null-ness and non-emptiness.</param>
    extension([NotNullWhen(true)] IEnumerable? collection)
    {
        /// <summary>
        ///     Determines whether the specified collection is not null and contains at least one element.
        /// </summary>
        /// <returns>
        ///     true if the specified collection is not null and contains at least one element; otherwise,
        ///     false.
        /// </returns>
        /// <remarks>
        ///     This extension method can be used on any type that implements the <see cref="System.Collections.IEnumerable" />
        ///     interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsNotNullOrEmpty" /> method to check if an array is not
        ///     null and not empty.
        ///     <code><![CDATA[
        /// int[] nonEmptyArray = new int[] { 1, 2, 3 };
        /// int[] nullArray = null;
        /// bool isNotNullOrEmpty1 = nonEmptyArray.IsNotNullOrEmpty();
        /// bool isNotNullOrEmpty2 = nullArray.IsNotNullOrEmpty();
        /// Console.WriteLine(isNotNullOrEmpty1); // Output: True
        /// Console.WriteLine(isNotNullOrEmpty2); // Output: False
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotNullOrEmpty() => collection != null && collection.IsNotEmpty();
    }

    /// <param name="collection">The collection to check for non-nullness and non-emptiness.</param>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    extension<T>([NotNullWhen(true)] IEnumerable<T>? collection)
    {
        /// <summary>
        ///     Determines whether the specified collection is not null and contains at least one element.
        /// </summary>
        /// <returns>
        ///     true if the specified collection is not null and contains at least one element; otherwise,
        ///     false.
        /// </returns>
        /// <remarks>
        ///     This extension method can be used on any type that implements the
        ///     <see cref="System.Collections.Generic.IEnumerable{T}" /> interface, including arrays and lists.
        /// </remarks>
        /// <example>
        ///     The following code demonstrates how to use the <see cref="IsNotNullOrEmpty{T}(IEnumerable{T})" /> method to check
        ///     if a list is not null and not empty.
        ///     <code><![CDATA[
        /// List<int> nonEmptyList = new List<int> { 1, 2, 3 };
        /// List<int> nullList = null;
        /// bool isNotNullOrEmpty1 = nonEmptyList.IsNotNullOrEmpty();
        /// bool isNotNullOrEmpty2 = nullList.IsNotNullOrEmpty();
        /// Console.WriteLine(isNotNullOrEmpty1); // Output: True
        /// Console.WriteLine(isNotNullOrEmpty2); // Output: False
        /// ]]></code>
        /// </example>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNotNullOrEmpty() => collection != null && collection.IsNotEmpty();
    }
}