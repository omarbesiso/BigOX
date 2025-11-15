using System.Collections.Frozen;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="IReadOnlyDictionary{TKey,TValue}" />
///     objects.
/// </summary>
public static class ReadOnlyDictionaryExtensions
{
    /// <summary>
    ///     Returns an immutable <see cref="FrozenDictionary{TKey, TValue}" /> from the source dictionary,
    ///     or an empty frozen dictionary when the source is null.
    ///     Optimized to:
    ///     - avoid extra allocations when already frozen
    ///     - fast-path empty dictionaries
    ///     - preserve the key comparer when the source is a <see cref="Dictionary{TKey,TValue}" />
    /// </summary>
    /// <param name="source">The source dictionary or null.</param>
    /// <returns>A frozen (immutable) dictionary instance.</returns>
    public static IReadOnlyDictionary<string, object?> FreezeOrEmpty(this IReadOnlyDictionary<string, object?>? source)
    {
        switch (source)
        {
            case null:
                return FrozenDictionary<string, object?>.Empty;
            case FrozenDictionary<string, object?> frozen:
                return frozen;
        }

        if (source.Count == 0)
        {
            return FrozenDictionary<string, object?>.Empty;
        }

        if (source is Dictionary<string, object?> dict)
        {
            return dict.ToFrozenDictionary(dict.Comparer);
        }

        // Fall back to default comparer when the original comparer isn't available
        return source.ToFrozenDictionary();
    }
}