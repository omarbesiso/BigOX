using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a string’s length does not exceed <paramref name="maxLength" /> characters.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">
    ///     The string to validate. May be <see langword="null" />.
    /// </param>
    /// <param name="maxLength">
    ///     The inclusive maximum number of UTF-16 code units allowed in <paramref name="value" />.
    ///     Must be zero or a positive number.
    /// </param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> exceeds <paramref name="maxLength" />.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length is
    ///     ≤ <paramref name="maxLength" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and its
    ///     length is greater than <paramref name="maxLength" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? description = GetDescription();
    /// description = Guard.MaxLength(description, 100);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? MaxLength(
        string? value,
        int maxLength,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Ensure the max length itself is valid (non-negative).
        Minimum(maxLength, 0, exceptionMessage: "Maximum length must be a non-negative value.");

        // Nulls are permitted; caller must use NotNull* helpers if null is unacceptable.
        if (value is null)
        {
            return value;
        }

        // Length check.
        if (value.Length <= maxLength)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The length of '{paramName}' cannot exceed {maxLength} characters."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}