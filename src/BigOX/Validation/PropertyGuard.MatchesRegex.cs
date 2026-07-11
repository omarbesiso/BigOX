using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string is either <see langword="null" /> or matches the supplied
    ///     regular-expression <paramref name="pattern" />.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="pattern">Regular-expression pattern the value must satisfy.</param>
    /// <param name="propertyName">
    ///     Name of the property, auto-captured via <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the pattern match.
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
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _email;
    /// 
    /// public string? Email
    /// {
    ///     get => _email;
    ///     set => _email = PropertyGuard.MatchesRegex(value,
    ///         @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? MatchesRegex(
        string? value,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null) =>
        Guard.MatchesRegex(value, pattern, propertyName, exceptionMessage);

    /// <summary>
    ///     Ensures that a property string is either <see langword="null" /> or matches the supplied pre-built
    ///     <paramref name="regex" />.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="regex">The <see cref="Regex" /> the value must satisfy.</param>
    /// <param name="propertyName">
    ///     Name of the property, auto-captured via <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the pattern match.
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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? MatchesRegex(
        string? value,
        Regex regex,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null) =>
        Guard.MatchesRegex(value, regex, propertyName, exceptionMessage);

    /// <summary>
    ///     Ensures that a property string is either <see langword="null" /> or matches the supplied
    ///     <paramref name="pattern" /> within the specified <paramref name="matchTimeout" />.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="pattern">Regular-expression pattern the value must satisfy.</param>
    /// <param name="matchTimeout">
    ///     The maximum time the match is allowed to run before a <see cref="RegexMatchTimeoutException" /> is thrown.
    /// </param>
    /// <param name="propertyName">
    ///     Name of the property, auto-captured via <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> fails the pattern match.
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
    ///     Thrown when <paramref name="matchTimeout" /> is out of the range accepted by <see cref="Regex" />.
    /// </exception>
    /// <exception cref="RegexMatchTimeoutException">
    ///     Thrown when the match runs longer than <paramref name="matchTimeout" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? MatchesRegex(
        string? value,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern,
        TimeSpan matchTimeout,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null) =>
        Guard.MatchesRegex(value, pattern, matchTimeout, propertyName, exceptionMessage);
}