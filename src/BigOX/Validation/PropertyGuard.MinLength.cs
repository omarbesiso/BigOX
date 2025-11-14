using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string’s length is **at least** <paramref name="minLength" /> characters.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="minLength">Inclusive minimum length in UTF-16 code units.</param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string is shorter than <paramref name="minLength" />.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length meets the requirement.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and its length is less than
    ///     <paramref name="minLength" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _code;
    /// 
    /// public string? Code
    /// {
    ///     get => _code;
    ///     set => _code = PropertyGuard.MinLength(value, 5);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? MinLength(
        string? value,
        int minLength,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.MinLength(value, minLength, propertyName, exceptionMessage);
    }
}