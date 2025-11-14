using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a <see cref="DateTime" /> value represents a moment
    ///     <strong>later than “now”</strong> (i.e. in the future).
    ///     The comparison is performed in the specified <paramref name="timeZone" />—or UTC when
    ///     <see langword="null" /> is supplied.
    /// </summary>
    /// <param name="value">The timestamp to validate.</param>
    /// <param name="timeZone">
    ///     Optional comparison time-zone.
    ///     • <see langword="null" /> (default) → UTC.
    ///     • <see cref="TimeZoneInfo.Local" /> → server’s local zone.
    ///     • Any other <see cref="TimeZoneInfo" /> instance → that zone.
    /// </param>
    /// <param name="paramName">
    ///     Argument name, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is not in the future.
    /// </param>
    /// <returns>The original <paramref name="value" /> when it lies in the future.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is in the past or present relative to the
    ///     chosen <paramref name="timeZone" />.
    /// </exception>
    /// <remarks>
    ///     If <paramref name="value" /><c>.Kind == DateTimeKind.Unspecified</c>, the guard
    ///     assumes the value is already expressed in <paramref name="timeZone" /> and
    ///     performs a direct comparison.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// DateTime scheduleAt = Guard.InFuture(requestedTime, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime InFuture(
        DateTime value,
        TimeZoneInfo? timeZone = null,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        timeZone ??= TimeZoneInfo.Utc;
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);

        var valueInZone = value.Kind switch
        {
            DateTimeKind.Utc => TimeZoneInfo.ConvertTime(value, timeZone),
            DateTimeKind.Local => TimeZoneInfo.ConvertTime(value, timeZone),
            _ => value
        };

        if (valueInZone > now)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must be in the future (zone: {timeZone.DisplayName})."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}