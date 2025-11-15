using System.Runtime.CompilerServices;
using System.Text;
using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides allocation-conscious extension methods for <see cref="StringBuilder" />.
/// </summary>
public static class StringBuilderExtensions
{
    // --- Internal helpers (avoid substring allocations) ---

    private static bool StartsWith(StringBuilder sb, string prefix, StringComparison comparison)
    {
        if (prefix.Length > sb.Length)
        {
            return false;
        }

        // Fast ordinal paths (case-sensitive / insensitive) without allocation
        if (comparison is StringComparison.Ordinal)
        {
            for (var i = 0; i < prefix.Length; i++)
            {
                if (sb[i] != prefix[i])
                {
                    return false;
                }
            }

            return true;
        }

        if (comparison is StringComparison.OrdinalIgnoreCase)
        {
            for (var i = 0; i < prefix.Length; i++)
            {
                if (char.ToUpperInvariant(sb[i]) != char.ToUpperInvariant(prefix[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Fallback: allocate minimal substring then compare using requested culture rules.
        return sb.ToString(0, prefix.Length).Equals(prefix, comparison);
    }

    private static bool EndsWith(StringBuilder sb, string suffix, StringComparison comparison)
    {
        if (suffix.Length > sb.Length)
        {
            return false;
        }

        var start = sb.Length - suffix.Length;

        if (comparison is StringComparison.Ordinal)
        {
            for (var i = 0; i < suffix.Length; i++)
            {
                if (sb[start + i] != suffix[i])
                {
                    return false;
                }
            }

            return true;
        }

        if (comparison is StringComparison.OrdinalIgnoreCase)
        {
            for (var i = 0; i < suffix.Length; i++)
            {
                if (char.ToUpperInvariant(sb[start + i]) != char.ToUpperInvariant(suffix[i]))
                {
                    return false;
                }
            }

            return true;
        }

        return sb.ToString(start, suffix.Length).Equals(suffix, comparison);
    }

    /// <param name="stringBuilder">The builder to examine.</param>
    extension(StringBuilder stringBuilder)
    {
        /// <summary>
        ///     Determines whether the <see cref="StringBuilder" /> is empty, optionally treating all whitespace as empty.
        /// </summary>
        /// <param name="countWhiteSpace">
        ///     If <c>true</c>, returns <c>true</c> when the builder contains only Unicode whitespace;
        ///     otherwise checks only for length 0.
        /// </param>
        /// <returns><c>true</c> if empty (per <paramref name="countWhiteSpace" />); otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        /// <remarks>Whitespace detection uses <see cref="char.IsWhiteSpace(char)" /> (Unicode). </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsEmpty(bool countWhiteSpace = false)
        {
            Guard.NotNull(stringBuilder);

            if (!countWhiteSpace)
            {
                return stringBuilder.Length == 0;
            }

            // Scan until a non-whitespace is found (early exit). Avoid allocating interim strings.
            for (var i = 0; i < stringBuilder.Length; i++)
            {
                if (!char.IsWhiteSpace(stringBuilder[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Appends <paramref name="charToAppend" /> until <see cref="StringBuilder.Length" /> reaches
        ///     <paramref name="targetLength" />.
        /// </summary>
        /// <param name="targetLength">The desired minimum length (&gt;= 0).</param>
        /// <param name="charToAppend">The character to append.</param>
        /// <returns>The same <see cref="StringBuilder" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="targetLength" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder AppendCharToLength(int targetLength,
            char charToAppend)
        {
            Guard.NotNull(stringBuilder);
            Guard.Minimum(targetLength, 0);

            var repeat = targetLength - stringBuilder.Length;
            if (repeat > 0)
            {
                stringBuilder.Append(charToAppend, repeat);
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Truncates the builder to <paramref name="maxLength" />; clears when <paramref name="maxLength" /> is 0.
        /// </summary>
        /// <param name="maxLength">The maximum length (&gt;= 0).</param>
        /// <returns>The same <see cref="StringBuilder" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="maxLength" /> is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder ReduceToLength(int maxLength)
        {
            Guard.NotNull(stringBuilder);
            Guard.Minimum(maxLength, 0);

            if (maxLength == 0)
            {
                stringBuilder.Clear();
            }
            else if (maxLength < stringBuilder.Length)
            {
                stringBuilder.Length = maxLength;
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Reverses the builder contents in place (by UTF-16 <see cref="char" /> code units).
        /// </summary>
        /// <returns>The same <see cref="StringBuilder" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        /// <remarks>
        ///     Does not preserve grapheme clusters (surrogate pairs / combining marks). See
        ///     <see cref="System.Globalization.StringInfo" /> for text-element aware operations.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StringBuilder Reverse()
        {
            Guard.NotNull(stringBuilder);

            for (int i = 0, j = stringBuilder.Length - 1; i < j; i++, j--)
            {
                (stringBuilder[i], stringBuilder[j]) = (stringBuilder[j], stringBuilder[i]);
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Ensures the builder starts with <paramref name="prefix" /> using the given comparison rules.
        /// </summary>
        /// <param name="prefix">The required prefix (non-empty).</param>
        /// <param name="stringComparison">Comparison to use; default is <see cref="StringComparison.InvariantCulture" />.</param>
        /// <returns>The same <see cref="StringBuilder" /> instance (inserting <paramref name="prefix" /> when needed).</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when stringBuilder or <paramref name="prefix" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="prefix" /> is empty.</exception>
        public StringBuilder EnsureStartsWith(string prefix,
            StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            Guard.NotNull(stringBuilder);
            Guard.NotNullOrEmpty(prefix);

            if (!StartsWith(stringBuilder, prefix, stringComparison))
            {
                stringBuilder.Insert(0, prefix);
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Ensures the builder ends with <paramref name="suffix" /> using the given comparison rules.
        /// </summary>
        /// <param name="suffix">The required suffix (non-empty).</param>
        /// <param name="stringComparison">Comparison to use; default is <see cref="StringComparison.InvariantCulture" />.</param>
        /// <returns>The same <see cref="StringBuilder" /> instance (appending <paramref name="suffix" /> when needed).</returns>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when stringBuilder or <paramref name="suffix" /> is
        ///     <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="suffix" /> is empty.</exception>
        public StringBuilder EnsureEndsWith(string suffix,
            StringComparison stringComparison = StringComparison.InvariantCulture)
        {
            Guard.NotNull(stringBuilder);
            Guard.NotNullOrEmpty(suffix);

            if (!EndsWith(stringBuilder, suffix, stringComparison))
            {
                stringBuilder.Append(suffix);
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Appends multiple strings, optionally inserting a line terminator <em>between</em> items (no trailing newline).
        /// </summary>
        /// <param name="withNewLine">If <c>true</c>, insert <see cref="Environment.NewLine" /> between non-empty items.</param>
        /// <param name="items">Strings to append; <c>null</c> and empty entries are skipped.</param>
        /// <returns>The same <see cref="StringBuilder" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        public StringBuilder AppendMultiple(bool withNewLine = false,
            params string?[]? items)
        {
            Guard.NotNull(stringBuilder);

            if (items is null || items.Length == 0)
            {
                return stringBuilder;
            }

            var wroteAny = false;
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }

                if (withNewLine && wroteAny)
                {
                    stringBuilder.AppendLine();
                }

                stringBuilder.Append(item);
                wroteAny = true;
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Removes all occurrences of <paramref name="characterToBeRemoved" /> in place.
        /// </summary>
        /// <param name="characterToBeRemoved">The character to remove.</param>
        /// <returns>The same <see cref="StringBuilder" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        public StringBuilder RemoveAllOccurrences(char characterToBeRemoved)
        {
            Guard.NotNull(stringBuilder);

            var length = stringBuilder.Length;
            if (length == 0)
            {
                return stringBuilder; // fast-path
            }

            var write = 0;
            for (var read = 0; read < length; read++)
            {
                var c = stringBuilder[read];
                if (c != characterToBeRemoved)
                {
                    stringBuilder[write++] = c;
                }
            }

            if (write != length)
            {
                stringBuilder.Length = write; // shrink only when any removed
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Trims leading and trailing Unicode whitespace in place.
        /// </summary>
        /// <returns>The same <see cref="StringBuilder" /> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        public StringBuilder Trim()
        {
            Guard.NotNull(stringBuilder);

            var len = stringBuilder.Length;
            var start = 0;
            while (start < len && char.IsWhiteSpace(stringBuilder[start]))
            {
                start++;
            }

            var endExclusive = len;
            while (endExclusive > start && char.IsWhiteSpace(stringBuilder[endExclusive - 1]))
            {
                endExclusive--;
            }

            if (start == 0 && endExclusive == len)
            {
                return stringBuilder; // nothing to trim
            }

            if (start == endExclusive)
            {
                stringBuilder.Clear(); // all whitespace
                return stringBuilder;
            }

            var newLen = endExclusive - start;

            if (start > 0)
            {
                // Shift characters left instead of Remove(0, start) to avoid internal re-allocation/copy twice
                for (var i = 0; i < newLen; i++)
                {
                    stringBuilder[i] = stringBuilder[start + i];
                }
            }

            if (stringBuilder.Length != newLen)
            {
                stringBuilder.Length = newLen;
            }

            return stringBuilder;
        }

        /// <summary>
        ///     Appends a formatted string followed by a platform line terminator.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="items">
        ///     Zero or more objects to format; when <c>null</c> or empty, <paramref name="format" /> is appended
        ///     verbatim.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when stringBuilder is <c>null</c> or
        ///     <paramref name="format" /> is null/whitespace.
        /// </exception>
        /// <remarks>
        ///     Uses the current culture; to control culture explicitly, call
        ///     <see cref="StringBuilder.AppendFormat(IFormatProvider?, string, object?)" /> overloads before
        ///     <see cref="StringBuilder.AppendLine()" />. AppendLine uses <see cref="Environment.NewLine" />.
        /// </remarks>
        public void AppendFormatLine(string format, params object?[]? items)
        {
            Guard.NotNull(stringBuilder);
            Guard.NotNullOrWhiteSpace(format);

            if (items is { Length: > 0 })
            {
                stringBuilder.AppendFormat(format, items);
            }
            else
            {
                stringBuilder.Append(format);
            }

            stringBuilder.AppendLine();
        }

        /// <summary>
        ///     Appends <paramref name="numberOfLines" /> platform line terminators.
        /// </summary>
        /// <param name="numberOfLines">Number of lines (&gt;= 1).</param>
        /// <exception cref="ArgumentNullException">Thrown when stringBuilder is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="numberOfLines" /> is less than 1.</exception>
        public void AppendMultipleLines(int numberOfLines)
        {
            Guard.NotNull(stringBuilder);
            Guard.Minimum(numberOfLines, 1);

            for (var i = 0; i < numberOfLines; i++)
            {
                stringBuilder.AppendLine();
            }
        }
    }
}