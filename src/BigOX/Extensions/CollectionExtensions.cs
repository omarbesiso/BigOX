using System.Runtime.CompilerServices;
using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="ICollection{T}" /> objects.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    ///     Shuffles the elements of the specified collection using the Fisher–Yates shuffle algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to shuffle.</param>
    /// <param name="preserveOriginal">
    ///     Specifies whether to preserve the original collection. If <c>true</c>, the shuffle is performed on a copy of the
    ///     collection;
    ///     otherwise, the shuffle is performed on the original collection. Defaults to <c>false</c>.
    /// </param>
    /// <param name="random">
    ///     An instance of <see cref="Random" /> to use for shuffling.
    ///     If <c>null</c>, a shared (thread-safe in .NET 6+) or new <see cref="Random" /> instance is used, depending on
    ///     target
    ///     framework. Defaults to <c>null</c>.
    /// </param>
    /// <returns>
    ///     A shuffled collection. This can be either a new collection if <paramref name="preserveOriginal" /> is true, or the
    ///     original collection otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input collection is <c>null</c>.</exception>
    /// <exception cref="NotSupportedException">Thrown if the collection is read-only.</exception>
    /// <remarks>
    ///     This method uses the Fisher–Yates shuffle algorithm for an efficient and unbiased shuffle.
    ///     <para>
    ///         **Thread Safety:**
    ///         - Starting with .NET 6, <see cref="Random.Shared" /> is thread-safe.
    ///         - If you pass a custom <see cref="Random" /> instance, ensure it is safe to use across threads if accessing
    ///         concurrently.
    ///     </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ICollection<T> Shuffle<T>(
        this IList<T> collection,
        bool preserveOriginal = false,
        Random? random = null)
    {
        Guard.NotNull(collection);

        if (!preserveOriginal && collection.IsReadOnly)
        {
            throw new NotSupportedException("Cannot shuffle a read-only list in place.");
        }

        random ??= Random.Shared;

        var shuffledList = preserveOriginal ? new List<T>(collection) : collection;

        if (shuffledList.Count <= 1)
        {
            return shuffledList;
        }

        // Fisher-Yates shuffle
        for (var i = shuffledList.Count - 1; i > 0; i--)
        {
            var j = random.Next(0, i + 1);
            (shuffledList[i], shuffledList[j]) = (shuffledList[j], shuffledList[i]);
        }

        return shuffledList;
    }

    /// <summary>
    ///     Provides extension methods for <see cref="ICollection{T}" />.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to extend.</param>
    extension<T>(ICollection<T> collection)
    {
        /// <summary>
        ///     Adds a value to the collection if it does not already exist in the collection.
        /// </summary>
        /// <param name="value">The value to add to the collection.</param>
        /// <returns>
        ///     <c>true</c> if the value was added to the collection;
        ///     <c>false</c> if the value already exists in the collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if the collection is <c>null</c>.</exception>
        /// <exception cref="NotSupportedException">Thrown if the collection is read-only.</exception>
        /// <remarks>
        ///     This method checks if the collection already contains the given value. If not, the value is added.
        ///     For collections that implement <see cref="ISet{T}" />, <see cref="ISet{T}.Add" /> is used for efficiency.
        ///     <para>
        ///         **Thread Safety:** This method is not thread-safe. If the collection is accessed concurrently
        ///         by multiple threads, ensure proper synchronization to avoid race conditions.
        ///     </para>
        /// </remarks>
        public bool AddUnique(T value)
        {
            Guard.NotNull(collection);

            if (collection is ISet<T> set)
            {
                return set.Add(value);
            }

            if (collection.IsReadOnly)
            {
                throw new NotSupportedException("The collection is read-only.");
            }

            if (collection.Contains(value))
            {
                return false;
            }

            collection.Add(value);
            return true;
        }

        /// <summary>
        ///     Removes all the elements from a collection that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="predicate">The delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the collection.</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if either the collection or the <paramref name="predicate" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">Thrown if the collection is read-only.</exception>
        /// <remarks>
        ///     This method iterates through the collection and removes each element that matches the conditions
        ///     defined by the specified predicate.
        ///     <para>
        ///         For collections that implement <see cref="List{T}" />, it removes items using <see cref="List{T}.RemoveAll" />
        ///         for efficiency. For <see cref="IList{T}" /> (non-list-based), items are removed via
        ///         <see cref="IList{T}.RemoveAt" />
        ///         from the back to avoid reindexing overhead. For <see cref="ISet{T}" />, items are removed directly.
        ///     </para>
        ///     <para>
        ///         **Thread Safety:** This method is not thread-safe. If the collection is accessed concurrently,
        ///         ensure proper synchronization to avoid race conditions.
        ///     </para>
        /// </remarks>
        public int RemoveWhere(
            Predicate<T> predicate)
        {
            Guard.NotNull(collection);
            Guard.NotNull(predicate);

            if (collection.IsReadOnly)
            {
                throw new NotSupportedException("The collection is read-only.");
            }

            if (collection.Count == 0)
            {
                return 0;
            }

            // If it's a List<T>, use RemoveAll for speed
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (collection is List<T> list)
            {
                return list.RemoveAll(predicate);
            }

            // If it's an IList<T> but not a List<T>
            if (collection is IList<T> internalList)
            {
                var count = 0;
                for (var i = internalList.Count - 1; i >= 0; i--)
                {
                    // ReSharper disable once InvertIf
                    if (predicate(internalList[i]))
                    {
                        internalList.RemoveAt(i);
                        count++;
                    }
                }

                return count;
            }

            // If it's an ISet<T>, remove items directly
            if (collection is ISet<T> set)
            {
                var itemsToRemove = set.Where(x => predicate(x)).ToList();
                foreach (var item in itemsToRemove)
                {
                    set.Remove(item);
                }

                return itemsToRemove.Count;
            }

            // Fallback for other ICollection<T> implementations
            var toRemove = collection.Where(item => predicate(item)).ToList();
            foreach (var item in toRemove)
            {
                collection.Remove(item);
            }

            return toRemove.Count;
        }

        /// <summary>
        ///     Adds an element to the collection if it satisfies the specified predicate.
        /// </summary>
        /// <param name="value">The element to add to the collection.</param>
        /// <param name="predicate">The predicate that determines if the element should be added.</param>
        /// <returns>
        ///     <c>true</c> if the element is added to the collection;
        ///     <c>false</c> if the element does not satisfy the predicate and is not added.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the collection or the <paramref name="predicate" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException"> Thrown if the predicate is true and the collection is read-only.</exception>
        /// <remarks>
        ///     This method checks whether the provided value satisfies the predicate. If so, the value is added to the collection.
        ///     <para>
        ///         **Thread Safety:** This method is not thread-safe. If the collection is accessed concurrently,
        ///         ensure proper synchronization to avoid race conditions.
        ///     </para>
        /// </remarks>
        public bool AddIf(
            T value,
            Func<T, bool> predicate)
        {
            Guard.NotNull(collection);
            Guard.NotNull(predicate);

            if (!predicate(value))
            {
                return false;
            }

            if (collection.IsReadOnly)
            {
                throw new NotSupportedException("The collection is read-only.");
            }

            collection.Add(value);
            return true;
        }

        /// <summary>
        ///     Checks if the collection contains any of the specified elements.
        /// </summary>
        /// <param name="values">The elements to check in the collection.</param>
        /// <returns>
        ///     <c>true</c> if the collection contains any of the specified elements;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the collection is <c>null</c>.
        /// </exception>
        /// <remarks>
        ///     This method efficiently determines if any of the specified elements are present in the collection.
        ///     <para>
        ///         If the collection implements <see cref="ISet{T}" />, it uses the set's <c>Contains</c> method for efficient
        ///         lookups.
        ///         Otherwise, it adjusts the strategy based on the sizes of the collection and the <paramref name="values" />
        ///         array.
        ///     </para>
        /// </remarks>
        public bool ContainsAny(
            params T[]? values)
        {
            return values is not null && ContainsAny(collection, values.AsSpan());
        }

        /// <summary>
        ///     Checks if the collection contains any of the specified elements.
        /// </summary>
        /// <param name="values">The elements to check in the collection.</param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the collection is <c>null</c>.
        /// </exception>
        /// <returns>
        ///     <c>true</c> if the collection contains any of the specified elements;
        ///     <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        ///     This method efficiently determines if any of the specified elements are present in the collection.
        ///     <para>
        ///         If the collection implements <see cref="ISet{T}" />, it uses the set's <c>Contains</c> method for efficient
        ///         lookups.
        ///         Otherwise, it adjusts the strategy based on the sizes of the collection and the <paramref name="values" />
        ///         array.
        ///     </para>
        /// </remarks>
        public bool ContainsAny(
            scoped ReadOnlySpan<T> values)
        {
            Guard.NotNull(collection);
            if (values.Length == 0)
            {
                return false;
            }

            // Set: O(m) where m = values.Length
            if (collection is ISet<T> set)
            {
                foreach (var v in values)
                {
                    if (set.Contains(v))
                    {
                        return true;
                    }
                }

                return false;
            }

            // Heuristic: when values is small, iterate it and call collection.Contains
            const int smallValuesThreshold = 8;
            if (values.Length <= smallValuesThreshold || values.Length < collection.Count)
            {
                foreach (var v in values)
                {
                    if (collection.Contains(v))
                    {
                        return true;
                    }
                }

                return false;
            }

            // Otherwise, hash the (smaller) side and scan the other
            var valueSet = new HashSet<T>();
            foreach (var v in values)
            {
                valueSet.Add(v);
            }

            return collection.Any(item => valueSet.Contains(item));
        }

        /// <summary>
        ///     Adds a range of unique values to the collection.
        /// </summary>
        /// <param name="values">The values to add to the collection.</param>
        /// <returns>
        ///     The count of values successfully added to the collection.
        ///     If the values are already present in the collection, they are not added, and hence not counted.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the collection is <c>null</c>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        ///     Thrown if the collection is read-only.
        /// </exception>
        /// <remarks>
        ///     This method iterates over the provided values and adds each one to the collection if it does not already exist.
        ///     <para>
        ///         For collections that implement <see cref="ISet{T}" />, this operation uses <see cref="ISet{T}.Add" />,
        ///         which handles uniqueness checks more efficiently.
        ///     </para>
        ///     <para>
        ///         **Thread Safety:** This method is not thread-safe. If the collection is accessed concurrently,
        ///         ensure proper synchronization to avoid race conditions.
        ///     </para>
        /// </remarks>
        public int AddUniqueRange(
            IEnumerable<T>? values)
        {
            Guard.NotNull(collection);

            if (values == null)
            {
                return 0;
            }

            if (collection.IsReadOnly)
            {
                throw new NotSupportedException("Cannot add items to a read-only collection.");
            }

            // Avoid "modified during enumeration" when values == collection
            if (ReferenceEquals(values, collection))
            {
                values = collection.ToArray(); // snapshot
            }

            var added = 0;

            // Best case: set
            if (collection is ISet<T> set)
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var v in values)
                {
                    if (set.Add(v))
                    {
                        added++;
                    }
                }

                return added;
            }

            // If we can cheaply know the candidate count, and it's small, avoid building a large lookup
            if (values.TryGetNonEnumeratedCount(out var candidateCount) && candidateCount <= 8)
            {
                foreach (var v in values)
                {
                    if (collection.Contains(v))
                    {
                        continue;
                    }

                    collection.Add(v);
                    added++;
                }

                return added;
            }

            // Build a lookup once. If the collection already is a HashSet<T>, preserve its comparer.
            var existing =
                collection is HashSet<T> hs
                    ? new HashSet<T>(hs, hs.Comparer)
                    : new HashSet<T>(collection);

            foreach (var v in values)
            {
                if (!existing.Add(v))
                {
                    continue; // already present
                }

                collection.Add(v);
                added++;
            }

            return added;
        }
    }
}