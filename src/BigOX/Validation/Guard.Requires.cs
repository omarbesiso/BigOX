using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that <paramref name="value" /> satisfies the supplied <paramref name="predicate" />.
    /// </summary>
    /// <typeparam name="T">The compile-time type of the value being validated.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="predicate">
    ///     A user-defined condition that <paramref name="value" /> must satisfy.
    /// </param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the predicate.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it satisfies <paramref name="predicate" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="predicate" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> does **not** satisfy <paramref name="predicate" />.
    /// </exception>
    /// <remarks>
    ///     Use this helper for one-off or domain-specific checks that cannot be expressed
    ///     through the standard guard repertoire.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// int age = GetAge();
    /// Guard.Requires(age, n => n >= 18, nameof(age), "Customer must be an adult.");
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Requires<T>(
        T value,
        [NotNull] Predicate<T>? predicate,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Predicate must be supplied
        if (predicate is null)
        {
            ThrowHelper.ThrowArgumentNull(nameof(predicate));
        }

        if (predicate(value))
        {
            return value;
        }

        // Fail when predicate returns false
        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' does not meet the required condition."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}