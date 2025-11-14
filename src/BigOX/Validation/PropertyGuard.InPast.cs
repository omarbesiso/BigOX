using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a <see cref="DateTime" /> property value is not set to a future instant.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="timeZone">
    ///     Optional comparison zone; <see langword="null" /> means UTC.
    /// </param>
    /// <param name="propertyName">
    ///     Property name, auto-captured via <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> lies in the future.
    /// </param>
    /// <returns>The original <paramref name="value" /> when it is not in the future.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is in the future.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private DateTime _dob;
    /// 
    /// public DateTime DateOfBirth
    /// {
    ///     get => _dob;
    ///     set => _dob = PropertyGuard.InPast(value, TimeZoneInfo.Local);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime InPast(
        DateTime value,
        TimeZoneInfo? timeZone = null,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.InPast(value, timeZone, propertyName, exceptionMessage);
    }
}