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
    ///     Thrown when <paramref name="pattern" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="pattern" /> is empty, or when <paramref name="value" /> is
    ///     non-<see langword="null" /> and does **not** match <paramref name="pattern" />.
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
    [return: NotNullIfNotNull(nameof(value))]
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

    /// <summary>
    ///     Ensures that a <paramref name="value" /> either is <see langword="null" /> <strong>or</strong> matches the
    ///     supplied pre-built <paramref name="regex" />.
    ///     A <see langword="null" /> input is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="regex">The <see cref="Regex" /> the value must satisfy. Must be non-<see langword="null" />.</param>
    /// <param name="paramName">
    ///     Name of <paramref name="value" />, auto-captured via <see cref="CallerArgumentExpressionAttribute" />
    ///     when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the pattern match.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or matches <paramref name="regex" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="regex" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and does <strong>not</strong> match
    ///     <paramref name="regex" />.
    /// </exception>
    /// <exception cref="RegexMatchTimeoutException">
    ///     Thrown when a match timeout configured on <paramref name="regex" /> elapses before the match completes.
    /// </exception>
    /// <remarks>
    ///     Prefer this overload when reusing a cached, compiled or timeout-configured <see cref="Regex" /> instance to
    ///     avoid re-parsing the pattern on every call.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? MatchesRegex(
        string? value,
        Regex regex,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        ArgumentNullException.ThrowIfNull(regex);

        // Nulls are permitted.
        if (value is null)
        {
            return value;
        }

        if (regex.IsMatch(value))
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' does not match the required pattern."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }

    /// <summary>
    ///     Ensures that a <paramref name="value" /> either is <see langword="null" /> <strong>or</strong> matches the
    ///     supplied <paramref name="pattern" /> within the specified <paramref name="matchTimeout" />.
    ///     A <see langword="null" /> input is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="pattern">
    ///     The regular-expression pattern the value must satisfy. Must be non-<see langword="null" /> and non-empty.
    /// </param>
    /// <param name="matchTimeout">
    ///     The maximum time the match is allowed to run before a <see cref="RegexMatchTimeoutException" /> is thrown.
    ///     Use <see cref="Regex.InfiniteMatchTimeout" /> to disable the timeout.
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
    ///     Thrown when <paramref name="pattern" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="pattern" /> is empty, or when <paramref name="value" /> is
    ///     non-<see langword="null" /> and does <strong>not</strong> match <paramref name="pattern" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="matchTimeout" /> is negative (other than
    ///     <see cref="Regex.InfiniteMatchTimeout" />), zero, or greater than approximately 24 days.
    /// </exception>
    /// <exception cref="RegexMatchTimeoutException">
    ///     Thrown when the match runs longer than <paramref name="matchTimeout" />.
    /// </exception>
    /// <remarks>
    ///     Use this overload to bound the cost of matching against untrusted input and mitigate catastrophic
    ///     backtracking (ReDoS).
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? MatchesRegex(
        string? value,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern,
        TimeSpan matchTimeout,
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

        if (Regex.IsMatch(value, pattern, RegexOptions.None, matchTimeout))
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