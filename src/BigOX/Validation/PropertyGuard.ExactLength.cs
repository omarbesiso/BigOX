using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string’s length is <strong>exactly</strong>
    ///     <paramref name="exactLength" /> characters.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="exactLength">The required number of UTF-16 code units.</param>
    /// <param name="propertyName">
    ///     The name of the property being validated, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the string does not have the required length.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length equals
    ///     <paramref name="exactLength" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and its length
    ///     differs from <paramref name="exactLength" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _pin;
    /// 
    /// public string? Pin
    /// {
    ///     get => _pin;
    ///     set => _pin = PropertyGuard.ExactLength(value, 4);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? ExactLength(
        string? value,
        int exactLength,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.ExactLength(value, exactLength, propertyName, exceptionMessage);
    }
}