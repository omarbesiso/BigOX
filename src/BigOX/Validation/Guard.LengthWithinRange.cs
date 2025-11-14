using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a string’s length lies within the inclusive range
    ///     <paramref name="minLength" /> … <paramref name="maxLength" />.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="minLength">
    ///     Inclusive minimum number of UTF-16 code units. Must be ≥ 0 and ≤ <paramref name="maxLength" />.
    /// </param>
    /// <param name="maxLength">
    ///     Inclusive maximum number of UTF-16 code units. Must be &gt; 0.
    /// </param>
    /// <param name="paramName">
    ///     Name of the argument being validated, auto-captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message when <paramref name="value" /> violates the length range.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length is between
    ///     <paramref name="minLength" /> and <paramref name="maxLength" />, inclusive.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when:
    ///     <list type="bullet">
    ///         <item>
    ///             <description><paramref name="maxLength" /> ≤ 0</description>
    ///         </item>
    ///         <item>
    ///             <description><paramref name="minLength" /> &lt; 0</description>
    ///         </item>
    ///         <item>
    ///             <description><paramref name="minLength" /> &gt; <paramref name="maxLength" /></description>
    ///         </item>
    ///         <item>
    ///             <description><paramref name="value" /> is non-null and its length is outside the specified range</description>
    ///         </item>
    ///     </list>
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? note = GetNote();          // may be null
    /// note = Guard.LengthWithinRange(note, 5, 100);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? LengthWithinRange(
        string? value,
        int minLength,
        int maxLength,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Validate range configuration.
        if (maxLength <= 0)
        {
            ThrowHelper.ThrowArgument(nameof(maxLength),
                "The maximum length specified cannot be less than or equal to 0.");
        }

        if (minLength < 0)
        {
            ThrowHelper.ThrowArgument(nameof(minLength),
                "The minimum length specified cannot be less than 0.");
        }

        if (minLength > maxLength)
        {
            ThrowHelper.ThrowArgument(nameof(minLength),
                "The minimum length specified cannot be greater than the maximum length specified.");
        }

        // Nulls are permitted; caller must forbid null via other guards if required.
        if (value is null)
        {
            return value;
        }

        // Actual length check.
        if (value.Length < minLength || value.Length > maxLength)
        {
            var message = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The length of '{paramName}' must be between {minLength} and {maxLength} characters."
                : exceptionMessage;

            ThrowHelper.ThrowArgument(paramName, message);
        }

        return value;
    }
}