using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string’s length does not exceed <paramref name="maxLength" /> characters.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="maxLength">Inclusive maximum length in UTF-16 code units.</param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string exceeds <paramref name="maxLength" />.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length is within limit.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and its length exceeds
    ///     <paramref name="maxLength" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _notes;
    /// 
    /// public string? Notes
    /// {
    ///     get => _notes;
    ///     set => _notes = PropertyGuard.MaxLength(value, 256);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? MaxLength(
        string? value,
        int maxLength,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.MaxLength(value, maxLength, propertyName, exceptionMessage);
    }
}