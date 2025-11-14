using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a string’s length is **at least** <paramref name="minLength" /> characters.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">
    ///     The string to validate. May be <see langword="null" />.
    /// </param>
    /// <param name="minLength">
    ///     The inclusive minimum number of UTF-16 code units required in <paramref name="value" />.
    ///     Must be zero or a positive number.
    /// </param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is shorter than
    ///     <paramref name="minLength" />. If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length is
    ///     ≥ <paramref name="minLength" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and its
    ///     length is less than <paramref name="minLength" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? password = GetPassword();
    /// password = Guard.MinLength(password, 8);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? MinLength(
        string? value,
        int minLength,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Ensure the minimum length itself is valid (non-negative).
        Minimum(minLength, 0, exceptionMessage: "Minimum length must be a non-negative value.");

        // Nulls are permitted; caller must use NotNull* helpers if null is unacceptable.
        if (value is null)
        {
            return value;
        }

        // Length check.
        if (value.Length < minLength)
        {
            var message = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The length of '{paramName}' must be at least {minLength} characters."
                : exceptionMessage;

            ThrowHelper.ThrowArgument(paramName, message);
        }

        return value;
    }
}