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
    ///     <strong>not later than “now”</strong> (i.e. in the past or present).
    ///     The comparison is performed in the specified <paramref name="timeZone" />—or UTC by default.
    /// </summary>
    /// <param name="value">The timestamp to validate.</param>
    /// <param name="timeZone">
    ///     Optional target time-zone used for the comparison.
    ///     • <see langword="null" /> (default) compares in UTC.
    ///     • Pass <see cref="TimeZoneInfo.Local" /> to use the server’s local zone.
    ///     • Pass any other <see cref="TimeZoneInfo" /> to compare in that zone.
    /// </param>
    /// <param name="paramName">
    ///     Argument name, auto-captured via <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> lies in the future.
    /// </param>
    /// <returns>The original <paramref name="value" /> when it is not in the future.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> represents a future instant relative to the chosen time-zone.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         <b>Unspecified kind</b> — If <paramref name="value" /><c>.Kind == Unspecified</c>,
    ///         the guard assumes the value is already expressed in <paramref name="timeZone" /> and
    ///         performs a direct comparison.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// DateTime createdAt = GetCreatedAt();
    /// createdAt = Guard.InPast(createdAt, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime InPast(
        DateTime value,
        TimeZoneInfo? timeZone = null,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // Choose comparison zone.
        timeZone ??= TimeZoneInfo.Utc;
        var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZone);

        // Convert value into comparison zone when its Kind is UTC or Local.
        var valueInZone = value.Kind switch
        {
            DateTimeKind.Utc => TimeZoneInfo.ConvertTime(value, timeZone),
            DateTimeKind.Local => TimeZoneInfo.ConvertTime(value, timeZone),
            _ => value // Unspecified is treated as already in the target zone.
        };

        if (valueInZone <= now)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must not be in the future (zone: {timeZone.DisplayName})."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}