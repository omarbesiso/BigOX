using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a <paramref name="value" /> either is <see langword="null" /> **or** matches the supplied
    ///     regular-expression <paramref name="pattern" />.
    ///     A <see langword="null" /> input is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="pattern">
    ///     The regular-expression pattern the value must satisfy.
    ///     Must be non-<see langword="null" /> and non-empty.
    /// </param>
    /// <param name="paramName">
    ///     Name of <paramref name="value" />, auto-captured via <see cref="CallerArgumentExpressionAttribute" />
    ///     when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the pattern match.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or matches <paramref name="pattern" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="pattern" /> is <see langword="null" /> or empty.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and does **not** match
    ///     <paramref name="pattern" />.
    /// </exception>
    /// <remarks>
    ///     Use this helper for format validation such as phone numbers, email addresses, reference codes, etc.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? phone = GetPhoneNumber();
    /// phone = Guard.MatchesRegex(phone, @"^\+?\d{10,15}$");
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? MatchesRegex(
        string? value,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Ensure pattern itself is valid.
        NotNullOrEmpty(pattern);

        // Nulls are permitted.
        if (value is null)
        {
            return value;
        }

        // Pattern match check.
        if (Regex.IsMatch(value, pattern))
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' does not match the required pattern."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}