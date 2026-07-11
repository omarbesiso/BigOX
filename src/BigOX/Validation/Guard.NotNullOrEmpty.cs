using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

public static partial class Guard
{
    /// <summary>
    ///     Ensures that the specified <paramref name="value" /> is neither <see langword="null" /> nor an empty string (
    ///     <c>""</c>).
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string is <see langword="null" /> or empty.
    ///     If omitted, default messages are generated for each failure case.
    /// </param>
    /// <returns>The original, non-<see langword="null" />, non-empty <paramref name="value" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is empty.
    /// </exception>
    /// <remarks>
    ///     Use this helper to ensure that string inputs contain meaningful data before proceeding with business logic.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string name = GetName();
    /// name = Guard.NotNullOrEmpty(name);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static string NotNullOrEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Null check (fast fail)
        if (value is null)
        {
            var msg = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The value of '{paramName}' cannot be null."
                : exceptionMessage;

            ThrowHelper.ThrowArgumentNull(paramName, msg);
        }

        // Empty check
        // ReSharper disable once InvertIf
        if (value.Length == 0)
        {
            var msg = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The value of '{paramName}' cannot be empty."
                : exceptionMessage;

            ThrowHelper.ThrowArgument(paramName, msg);
        }

        return value;
    }

    /// <summary>
    ///     Ensures that the specified <paramref name="collection" /> is not <see langword="null" /> and
    ///     contains at least one element.
    ///     This method attempts the fastest check first (<c>Count</c> or
    ///     <see cref="Enumerable.TryGetNonEnumeratedCount{TSource}" />) and, if that is unavailable,
    ///     lazily wraps the source sequence so enumeration occurs exactly once.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the collection is <see langword="null" /> or empty.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="collection" /> reference when its count is available without
    ///     enumeration; otherwise a lazy wrapper over the sequence. In both cases the result is
    ///     non-<see langword="null" /> and non-empty.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="collection" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="collection" /> contains no elements.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         For collections that expose a count without enumeration — <see cref="ICollection{T}" />
    ///         (arrays, <see cref="List{T}" />, <see cref="HashSet{T}" />, etc.) and
    ///         <see cref="IReadOnlyCollection{T}" /> (including <see cref="IReadOnlyList{T}" />) — the
    ///         emptiness check is <c>O(1)</c>, performs **no** enumeration, and the original reference
    ///         is returned.
    ///     </para>
    ///     <para>
    ///         For pipeline / generator sequences a lazy wrapper is returned; the emptiness check is
    ///         deferred until the wrapper is first enumerated, at which point it reads **at most one**
    ///         element ahead and then yields the items so callers still enumerate exactly once.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// var items = Enumerable.Range(1, 5);
    /// var safe = Guard.NotNullOrEmpty(items); // succeeds
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> NotNullOrEmpty<T>(
        [NotNull] IEnumerable<T>? collection,
        [CallerArgumentExpression(nameof(collection))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Null short-circuit
        if (collection is null)
        {
            ThrowHelper.ThrowArgumentNull(paramName, exceptionMessage);
        }

        // Prefer an O(1) count when the source exposes one. TryGetNonEnumeratedCount covers
        // ICollection<T>, the non-generic ICollection and arrays; IReadOnlyCollection<T>
        // (e.g. IReadOnlyList<T>) is checked explicitly because it is not consulted otherwise.
        // Validating eagerly here means callers that discard the returned reference still get an
        // immediate emptiness check instead of a deferred one they might never trigger.
        int? knownCount =
            collection.TryGetNonEnumeratedCount(out var count) ? count
            : collection is IReadOnlyCollection<T> readOnly ? readOnly.Count
            : null;

        if (knownCount is { } availableCount)
        {
            if (availableCount == 0)
            {
                ThrowEmpty(paramName, exceptionMessage);
            }

            return collection;
        }

        return EnumerateOnce(collection, paramName, exceptionMessage);

        // Wrap the enumerator so the sequence is consumed only once

        static IEnumerable<T> EnumerateOnce(
            IEnumerable<T> src, string name, string? msg)
        {
            using var e = src.GetEnumerator();
            if (!e.MoveNext())
            {
                ThrowEmpty(name, msg);
            }

            do
            {
                yield return e.Current;
            } while (e.MoveNext());
        }
    }

    /// <summary>
    ///     Ensures that a nullable <see cref="Guid" /> is neither <see langword="null" /> nor
    ///     equal to <see cref="Guid.Empty" />.
    /// </summary>
    /// <param name="value">The nullable GUID to validate.</param>
    /// <param name="paramName">
    ///     Name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the GUID is <see langword="null" /> or empty.
    ///     If omitted, sensible defaults are generated.
    /// </param>
    /// <returns>The original, non-null, non-empty <see cref="Guid" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals <see cref="Guid.Empty" />.
    /// </exception>
    /// <remarks>
    ///     Useful for validating identifiers that may legitimately be <see langword="null" /> to indicate
    ///     “not supplied” yet must never be the default all-zeros value when present.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// Guid? tenantId = GetTenantId();
    /// Guid validated = Guard.NotNullOrEmpty(tenantId); // throws on null or Guid.Empty
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NotNullOrEmpty(
        Guid? value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Null check first (fast-fail)
        if (value is null)
        {
            ThrowHelper.ThrowArgumentNull(paramName, exceptionMessage);
        }

        // Empty check
        if (value.Value != Guid.Empty)
        {
            return value.Value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The GUID '{paramName}' cannot be empty."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value.Value;
    }

    [DoesNotReturn]
    [MethodImpl(MethodImplOptions.NoInlining)]
    [StackTraceHidden]
    private static void ThrowEmpty(string paramName, string? rawMsg)
    {
        var message = string.IsNullOrWhiteSpace(rawMsg)
            ? $"The value of '{paramName}' cannot be empty."
            : rawMsg;

        ThrowHelper.ThrowArgument(paramName, message);
    }
}