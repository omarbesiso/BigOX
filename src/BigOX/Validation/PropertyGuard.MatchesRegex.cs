using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
    ///     Thrown when <paramref name="pattern" /> is <see langword="null" /> or empty.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and does **not** match
    ///     <paramref name="pattern" />.
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
    public static string? MatchesRegex(
        string? value,
        [StringSyntax(StringSyntaxAttribute.Regex)]
        string pattern,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.MatchesRegex(value, pattern, propertyName, exceptionMessage);
    }
}