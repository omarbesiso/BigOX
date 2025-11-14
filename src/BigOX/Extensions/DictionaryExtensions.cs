using System.Diagnostics.Contracts;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="IDictionary{TKey,TValue}" /> objects.
/// </summary>
public static class DictionaryExtensions
{
    /// <param name="dictionary">
    ///     The <see cref="IDictionary{TKey, TValue}" /> to create the
    ///     <see cref="SortedDictionary{TKey, TValue}" /> from.
    /// </param>
    /// <typeparam name="TKey">The type of the keys in the dictionary. TKey must be a non-nullable value type.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        /// <summary>
        ///     Creates a new <see cref="SortedDictionary{TKey, TValue}" /> from the elements of the
        ///     <see cref="IDictionary{TKey, TValue}" />.
        /// </summary>
        /// <returns>
        ///     A new <see cref="SortedDictionary{TKey, TValue}" /> containing the same key-value pairs as the input
        ///     dictionary.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">Thrown when dictionary is <c>null</c>.</exception>
        /// <example>
        ///     <code><![CDATA[
        /// IDictionary<string, int> sampleDictionary = new Dictionary<string, int>
        /// {
        ///     { "One", 1 },
        ///     { "Two", 2 },
        ///     { "Three", 3 }
        /// };
        /// 
        /// SortedDictionary<string, int> sortedDictionary = sampleDictionary.ToSortedDictionary();
        /// 
        /// // The sortedDictionary now contains the same key-value pairs as sampleDictionary, sorted by key.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This method is an extension method and can be used directly on any object implementing
        ///     <see cref="System.Collections.Generic.IDictionary{TKey, TValue}" />. It creates a new
        ///     <see cref="SortedDictionary{TKey, TValue}" /> containing the same key-value pairs as
        ///     the input dictionary, sorted by key. The original dictionary remains unchanged.
        /// </remarks>
        [Pure]
        public SortedDictionary<TKey, TValue> ToSortedDictionary()
        {
            ArgumentNullException.ThrowIfNull(dictionary);

            return new SortedDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        ///     Creates a new <see cref="SortedDictionary{TKey, TValue}" /> from the specified dictionary using the specified key
        ///     comparer.
        /// </summary>
        /// <param name="comparer">The key comparer used to sort the keys in the resulting sorted dictionary.</param>
        /// <returns>
        ///     A new <see cref="SortedDictionary{TKey, TValue}" /> containing the same key-value pairs as the input
        ///     dictionary, sorted by the provided comparer.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown when the dictionary or
        ///     <paramref name="comparer" /> is <c>null</c>.
        /// </exception>
        /// <example>
        ///     <code><![CDATA[
        /// IDictionary<string, int> sampleDictionary = new Dictionary<string, int>
        /// {
        ///     { "One", 1 },
        ///     { "Two", 2 },
        ///     { "Three", 3 }
        /// };
        /// 
        /// IComparer<string> keyComparer = StringComparer.OrdinalIgnoreCase;
        /// SortedDictionary<string, int> sortedDictionary = sampleDictionary.ToSortedDictionary(keyComparer);
        /// 
        /// // The sortedDictionary now contains the same key-value pairs as sampleDictionary, sorted by key using the provided comparer.
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This method is an extension method and can be used directly on any object implementing
        ///     <see cref="System.Collections.Generic.IDictionary{TKey, TValue}" />. It creates a new
        ///     <see cref="SortedDictionary{TKey, TValue}" /> containing the same key-value pairs as the input dictionary, sorted
        ///     by the provided comparer. The original dictionary remains unchanged.
        /// </remarks>
        [Pure]
        public SortedDictionary<TKey, TValue> ToSortedDictionary(IComparer<TKey> comparer)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            ArgumentNullException.ThrowIfNull(comparer);

            return new SortedDictionary<TKey, TValue>(dictionary, comparer);
        }

        /// <summary>
        ///     Removes all elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <param name="predicate">The predicate that defines the conditions of the elements to remove.</param>
        public void RemoveWhere(Func<KeyValuePair<TKey, TValue>, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            ArgumentNullException.ThrowIfNull(predicate);

            if (dictionary.Count == 0)
            {
                return;
            }

            List<TKey>? keysToRemove = null;
            foreach (var kvp in dictionary)
            {
                if (predicate(kvp))
                {
                    keysToRemove ??= new List<TKey>();
                    keysToRemove.Add(kvp.Key);
                }
            }

            if (keysToRemove is null)
            {
                return;
            }

            foreach (var key in keysToRemove)
            {
                dictionary.Remove(key);
            }
        }

        /// <summary>
        ///     Merges another dictionary into the current dictionary. If the same key exists in both dictionaries,
        ///     the value from the other dictionary overwrites the value in the current dictionary by default.
        /// </summary>
        /// <param name="otherDictionary">The dictionary to merge from.</param>
        /// <param name="overwriteExisting">
        ///     A boolean value indicating whether to overwrite existing values in the current dictionary.
        ///     Default is <c>true</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when dictionary or <paramref name="otherDictionary" /> is <c>null</c>.
        /// </exception>
        /// <example>
        ///     <code><![CDATA[
        /// IDictionary<string, int> dictionary1 = new Dictionary<string, int>
        /// {
        ///     { "One", 1 },
        ///     { "Two", 2 }
        /// };
        /// 
        /// IDictionary<string, int> dictionary2 = new Dictionary<string, int>
        /// {
        ///     { "Two", 22 },
        ///     { "Three", 3 }
        /// };
        /// 
        /// dictionary1.Merge(dictionary2, overwriteExisting: true);
        /// // dictionary1 now contains: { "One", 1 }, { "Two", 22 }, { "Three", 3 }
        /// 
        /// dictionary1.Merge(dictionary2, overwriteExisting: false);
        /// // dictionary1 remains unchanged: { "One", 1 }, { "Two", 22 }, { "Three", 3 }
        /// ]]></code>
        /// </example>
        public void Merge(IDictionary<TKey, TValue> otherDictionary, bool overwriteExisting = true)
        {
            ArgumentNullException.ThrowIfNull(dictionary);
            ArgumentNullException.ThrowIfNull(otherDictionary);

            if (overwriteExisting)
            {
                foreach (var kvp in otherDictionary)
                {
                    dictionary[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                foreach (var kvp in otherDictionary)
                {
                    _ = dictionary.TryAdd(kvp.Key, kvp.Value);
                }
            }
        }
    }
}