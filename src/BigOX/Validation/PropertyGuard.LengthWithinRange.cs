using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property string’s length lies within the inclusive range
    ///     <paramref name="minLength" /> … <paramref name="maxLength" />.
    ///     A <see langword="null" /> value is considered valid and returned unchanged.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="minLength">Inclusive minimum length (≥ 0).</param>
    /// <param name="maxLength">Inclusive maximum length (&gt; 0).</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, auto-captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message when <paramref name="value" /> violates the length range.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or its length is within the specified range.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     See <see cref="Guard.LengthWithinRange" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private string? _summary;
    /// 
    /// public string? Summary
    /// {
    ///     get => _summary;
    ///     set => _summary = PropertyGuard.LengthWithinRange(value, 10, 200);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? LengthWithinRange(
        string? value,
        int minLength,
        int maxLength,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.LengthWithinRange(value, minLength, maxLength, propertyName, exceptionMessage);
    }
}