using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a string’s length is <strong>exactly</strong>
    ///     <paramref name="exactLength" /> characters.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="exactLength">
    ///     The required number of UTF-16 code units. Must be zero or a positive number.
    /// </param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> does not have the
    ///     required length. If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length equals
    ///     <paramref name="exactLength" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and its length
    ///     differs from <paramref name="exactLength" />.
    /// </exception>
    /// <remarks>
    ///     Use <see cref="MinLength" /> or <see cref="MaxLength" /> for range constraints, and
    ///     <see cref="NotNullOrWhiteSpace" /> when you must also forbid empty / whitespace strings.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? code = GetVerificationCode();   // may be null
    /// code = Guard.ExactLength(code, 6);      // throws if non-null and not 6 characters
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ExactLength(
        string? value,
        int exactLength,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Ensure the supplied required length is non-negative.
        Minimum(exactLength, 0, exceptionMessage: "Exact length must be a non-negative value.");

        // Nulls are permitted; caller should use NotNull* helpers if null is unacceptable.
        if (value is null)
        {
            return value;
        }

        // Length check.
        if (value.Length != exactLength)
        {
            var message = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The length of '{paramName}' must be exactly {exactLength} characters."
                : exceptionMessage;

            ThrowHelper.ThrowArgument(paramName, message);
        }

        return value;
    }
}