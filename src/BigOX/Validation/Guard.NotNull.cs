using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Validation utilities used in a code-contract style to assert common argument pre-conditions.
/// </summary>
/// <remarks>
///     All members are static and inline-friendly, delegating exceptional paths to
///     <see cref="System.ThrowHelper" /> to keep hot code fast.
/// </remarks>
[DebuggerStepThrough]
[StackTraceHidden]
public static partial class Guard
{
    /// <summary>
    ///     Ensures that <paramref name="value" /> is not <see langword="null" />.
    /// </summary>
    /// <typeparam name="T">
    ///     The compile-time type of the argument.
    ///     Leave unconstrained so the helper can be used with reference or nullable value types.
    /// </typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">
    ///     The argument name, automatically captured by
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message propagated to the <see cref="ArgumentNullException" />.
    /// </param>
    /// <returns>The non-null <paramref name="value" /> for fluent use.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <example>
    ///     <code><![CDATA[
    /// // Throws if title is null
    /// title = Guard.NotNull(title);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNull]
    public static T NotNull<T>(
        [NotNull] [DoesNotReturnIf(true)] T? value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        if (value is null)
        {
            ThrowHelper.ThrowArgumentNull(paramName, exceptionMessage);
        }

        return value;
    }

    /// <summary>
    ///     Ensures that <paramref name="collection" /> is not <see langword="null" />.
    /// </summary>
    /// <typeparam name="T">Element type of the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">
    ///     Name of the argument being validated, captured automatically by
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message. When omitted, a default message is generated.
    /// </param>
    /// <returns>The non-null <paramref name="collection" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="collection" /> is <see langword="null" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<T> NotNull<T>(
        [NotNull] [DoesNotReturnIf(true)] IEnumerable<T>? collection,
        [CallerArgumentExpression(nameof(collection))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        if (collection is null)
        {
            ThrowHelper.ThrowArgumentNull(paramName, exceptionMessage);
        }

        return collection;
    }
}