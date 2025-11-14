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
    ///     Ensures that a property string is neither <see langword="null" />, empty, nor white-space.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string is <see langword="null" />, empty, or white-space.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it contains at least one non-white-space character.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="value" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is empty or consists only of white-space characters.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string _title = string.Empty;
    /// 
    /// public string Title
    /// {
    ///     get => _title;
    ///     set => _title = PropertyGuard.NotNullOrWhiteSpace(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string NotNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.NotNullOrWhiteSpace(value, propertyName, exceptionMessage);
    }
}