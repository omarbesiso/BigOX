using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a <see cref="DateTime" /> property value represents a moment in the future.
    /// </summary>
    /// <param name="value">The property value to validate.</param>
    /// <param name="timeZone">
    ///     Optional comparison time-zone; <see langword="null" /> means UTC.
    /// </param>
    /// <param name="propertyName">
    ///     Property name, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is not in the future.
    /// </param>
    /// <returns>The original <paramref name="value" /> when it lies in the future.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is in the past or present relative to the
    ///     chosen <paramref name="timeZone" />.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private DateTime _meetingAt;
    /// 
    /// public DateTime MeetingAt
    /// {
    ///     get => _meetingAt;
    ///     set => _meetingAt = PropertyGuard.InFuture(value, TimeZoneInfo.Local);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime InFuture(
        DateTime value,
        TimeZoneInfo? timeZone = null,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
    {
        return Guard.InFuture(value, timeZone, propertyName, exceptionMessage);
    }
}