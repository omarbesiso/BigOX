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
    ///     Ensures that a property string is either <see langword="null" /> or a valid absolute HTTP/HTTPS URL.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is non-null and invalid.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or a valid absolute HTTP/HTTPS URL.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and not a valid absolute HTTP/HTTPS URL.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _website;
    /// 
    /// public string? Website
    /// {
    ///     get => _website;
    ///     set => _website = PropertyGuard.Url(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Url(
        string? value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null) =>
        Guard.Url(value, propertyName, exceptionMessage);

    /// <summary>
    ///     Ensures that a property string is either <see langword="null" /> or a valid absolute URL whose scheme is one
    ///     of <paramref name="allowedSchemes" /> (compared case-insensitively).
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="allowedSchemes">
    ///     The set of permitted URI schemes. Must be non-<see langword="null" /> and contain at least one element.
    /// </param>
    /// <param name="propertyName">
    ///     Name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is non-null and invalid.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or a valid absolute URL using an allowed scheme.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="allowedSchemes" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="allowedSchemes" /> is empty, or when <paramref name="value" /> is
    ///     non-<see langword="null" /> and is not a valid absolute URL using one of the allowed schemes.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Url(
        string? value,
        string[] allowedSchemes,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null) =>
        Guard.Url(value, allowedSchemes, propertyName, exceptionMessage);
}