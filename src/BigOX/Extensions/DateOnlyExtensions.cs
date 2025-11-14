using System.Globalization;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="DateOnly" /> objects.
/// </summary>
public static class DateOnlyExtensions
{
    /// <summary>
    ///     Provides extension methods for the <see cref="DateOnly" /> struct.
    /// </summary>
    /// <param name="date">The date for which to find the previous day.</param>
    extension(DateOnly date)
    {
        /// <summary>
        ///     Returns a new <see cref="DateOnly" /> instance representing the day before the source date.
        /// </summary>
        /// <returns>
        ///     A new <see cref="DateOnly" /> instance representing the day before the source date.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the source date is <see cref="DateOnly.MinValue" /> and cannot have a previous day.
        /// </exception>
        /// <remarks>
        ///     Subtracts one day from the source <see cref="DateOnly" /> instance to calculate the previous day.
        ///     If the source date is <see cref="DateOnly.MinValue" />, an <see cref="InvalidOperationException" /> is thrown.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// var today = DateOnly.Today;
        /// var yesterday = today.PreviousDay(); // Yesterday's date
        /// 
        /// // Edge case:
        /// var minDate = DateOnly.MinValue;
        /// // Throws InvalidOperationException
        /// var previousDay = minDate.PreviousDay();
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly PreviousDay()
        {
            return date == DateOnly.MinValue
                ? throw new InvalidOperationException("Cannot get the previous day of DateOnly.MinValue.")
                : date.AddDays(-1);
        }

        /// <summary>
        ///     Returns a new <see cref="DateOnly" /> instance representing the day after the source date.
        /// </summary>
        /// <returns>
        ///     A new <see cref="DateOnly" /> instance representing the day after the source date.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown when the source date is <see cref="DateOnly.MaxValue" /> and cannot have a next day.
        /// </exception>
        /// <remarks>
        ///     Adds one day to the source <see cref="DateOnly" /> instance to calculate the next day.
        ///     If the source date is <see cref="DateOnly.MaxValue" />, an <see cref="InvalidOperationException" /> is thrown.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// var today = DateOnly.Today;
        /// var tomorrow = today.NextDay(); // Tomorrow's date
        /// 
        /// // Edge case:
        /// var maxDate = DateOnly.MaxValue;
        /// // Throws InvalidOperationException
        /// var nextDay = maxDate.NextDay();
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly NextDay()
        {
            return date == DateOnly.MaxValue
                ? throw new InvalidOperationException("Cannot get the next day of DateOnly.MaxValue.")
                : date.AddDays(1);
        }

        /// <summary>
        ///     Returns an enumerable of <see cref="DateOnly" /> instances representing the dates in the range between the
        ///     source date and <paramref name="toDate" />, inclusive.
        /// </summary>
        /// <param name="toDate">The end date of the range.</param>
        /// <returns>
        ///     An enumerable of <see cref="DateOnly" /> instances representing the dates in the range between the source
        ///     date and <paramref name="toDate" />, inclusive.
        /// </returns>
        /// <remarks>
        ///     This method generates an enumerable of dates from the source date to <paramref name="toDate" />,
        ///     inclusive, in either ascending or descending order depending on the date values.
        /// </remarks>
        /// <example>
        ///     Ascending date range:
        ///     <code><![CDATA[
        /// DateOnly startDate = new(2023, 1, 1);
        /// DateOnly endDate = new(2023, 1, 5);
        /// IEnumerable<DateOnly> dateRange = startDate.GetDatesInRange(endDate);
        /// // dateRange will contain dates from January 1st to January 5th, 2023.
        /// ]]></code>
        ///     Descending date range:
        ///     <code><![CDATA[
        /// DateOnly startDate = new(2023, 1, 5);
        /// DateOnly endDate = new(2023, 1, 1);
        /// IEnumerable<DateOnly> dateRange = startDate.GetDatesInRange(endDate);
        /// // dateRange will contain dates from January 5th to January 1st, 2023.
        /// ]]></code>
        ///     Single date:
        ///     <code><![CDATA[
        /// DateOnly date = new(2023, 1, 1);
        /// IEnumerable<DateOnly> dateRange = date.GetDatesInRange(date);
        /// // dateRange will contain only January 1st, 2023.
        /// ]]></code>
        /// </example>
        public IEnumerable<DateOnly> GetDatesInRange(DateOnly toDate)
        {
            var step = Math.Sign(toDate.DayNumber - date.DayNumber);
            if (step == 0)
            {
                yield return date;
                yield break;
            }

            for (var dt = date;; dt = dt.AddDays(step))
            {
                yield return dt;
                if (dt == toDate)
                {
                    yield break;
                }
            }
        }

        /// <summary>
        ///     Determines whether the source date is between the specified <paramref name="rangeStart" />
        ///     and <paramref name="rangeEnd" /> dates.
        /// </summary>
        /// <param name="rangeStart">The start date of the range.</param>
        /// <param name="rangeEnd">The end date of the range.</param>
        /// <param name="isInclusive">
        ///     A boolean value indicating whether the range is inclusive or exclusive. Default is
        ///     <see langword="true" /> (inclusive).
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the source date is within the specified range; otherwise,
        ///     <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The order of the range dates does not matter; the method will automatically
        ///     handle cases where <paramref name="rangeStart" /> is after <paramref name="rangeEnd" />.
        ///     If <paramref name="isInclusive" /> is <see langword="true" />, the range is considered
        ///     inclusive, and the method will return <see langword="true" /> if the source date is equal to either
        ///     <paramref name="rangeStart" /> or <paramref name="rangeEnd" />. If <paramref name="isInclusive" /> is
        ///     <see langword="false" />, the range is considered exclusive, and the method will return
        ///     <see langword="false" /> if the source date is equal to either <paramref name="rangeStart" /> or
        ///     <paramref name="rangeEnd" />.
        /// </remarks>
        /// <example>
        ///     Ascending range example:
        ///     <code><![CDATA[
        /// DateOnly date = new DateOnly(2023, 1, 15);
        /// DateOnly rangeStart = new DateOnly(2023, 1, 1);
        /// DateOnly rangeEnd = new DateOnly(2023, 1, 31);
        /// bool result = date.IsBetween(rangeStart, rangeEnd);
        /// // result will be true
        ///     ]]></code>
        ///     Descending range example:
        ///     <code><![CDATA[
        /// DateOnly date = new DateOnly(2023, 1, 15);
        /// DateOnly rangeStart = new DateOnly(2023, 1, 31);
        /// DateOnly rangeEnd = new DateOnly(2023, 1, 1);
        /// bool result = date.IsBetween(rangeStart, rangeEnd);
        /// // result will be true
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBetween(DateOnly rangeStart, DateOnly rangeEnd, bool isInclusive = true)
        {
            if (rangeStart > rangeEnd)
            {
                // Swap the range to ensure rangeStart <= rangeEnd
                (rangeStart, rangeEnd) = (rangeEnd, rangeStart);
            }

            return isInclusive
                ? date >= rangeStart && date <= rangeEnd
                : date > rangeStart && date < rangeEnd;
        }

        /// <summary>
        ///     Converts the source <see cref="DateOnly" /> instance to a <see cref="DateTime" /> instance
        ///     with the time set to midnight (00:00:00) and the kind set to <see cref="DateTimeKind.Unspecified" />.
        /// </summary>
        /// <returns>
        ///     A <see cref="DateTime" /> instance representing the same date as the source
        ///     <see cref="DateOnly" /> instance, with the time set to midnight (00:00:00)
        ///     and the kind set to <see cref="DateTimeKind.Unspecified" />.
        /// </returns>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly date = new DateOnly(2023, 1, 1);
        /// DateTime dateTime = date.ToDateTime();
        /// // dateTime will be January 1, 2023, 00:00:00 with DateTimeKind.Unspecified
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This method converts a <see cref="DateOnly" /> instance to a <see cref="DateTime" /> instance
        ///     by setting the time to midnight (00:00:00) and the kind to <see cref="DateTimeKind.Unspecified" />.
        ///     This is useful when you need to work with APIs or libraries that require <see cref="DateTime" /> instances
        ///     but you only have date information.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ToDateTime()
        {
            return date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        }

        /// <summary>
        ///     Converts the source <see cref="DateOnly" /> instance to a <see cref="DateTime" /> instance
        ///     using the provided <paramref name="time" /> and <paramref name="kind" />.
        /// </summary>
        /// <param name="time">The <see cref="TimeOnly" /> that specifies the time of day.</param>
        /// <param name="kind">Specifies whether the converted <see cref="DateTime" /> is UTC, local time, or unspecified.</param>
        /// <returns>
        ///     A <see cref="DateTime" /> instance representing the combined date and time, with the specified
        ///     <see cref="DateTimeKind" />.
        /// </returns>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly date = new DateOnly(2023, 1, 1);
        /// TimeOnly time = new TimeOnly(13, 30); // 1:30 PM
        /// DateTime utcDateTime = date.ToDateTime(time, DateTimeKind.Utc);
        /// // utcDateTime will be January 1, 2023, 13:30:00 UTC
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     This overload allows you to specify a custom time of day or <see cref="DateTimeKind" />,
        ///     which can be useful for scenarios where you need more control over the resulting <see cref="DateTime" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime ToDateTime(TimeOnly time, DateTimeKind kind)
        {
            // Safety check: if for any reason the consumer passes default here, just treat it as midnight
            if (time == default)
            {
                time = TimeOnly.MinValue;
            }

            return date.ToDateTime(time, kind);
        }

        /// <summary>
        ///     Calculates the age based on the date of birth, an optional maturity date, and an optional time zone.
        /// </summary>
        /// <param name="maturityDate">
        ///     An optional maturity date to calculate the age. If not provided, the current date in the specified time zone is
        ///     used.
        /// </param>
        /// <param name="timeZoneInfo">
        ///     An optional time zone to determine the current date if <paramref name="maturityDate" /> is <c>null</c>.
        ///     If not provided, the local system time zone is used.
        /// </param>
        /// <returns>The calculated age in years.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the source date (date of birth) is in the future relative to the maturity date or current date.
        /// </exception>
        /// <remarks>
        ///     This method calculates the age in years based on full years elapsed, accounting for birthdays.
        ///     If the <paramref name="maturityDate" /> is <c>null</c>, the current date in the specified time zone is used.
        ///     If the <paramref name="timeZoneInfo" /> is <c>null</c>, the local system time zone is used.
        /// </remarks>
        /// <example>
        ///     The following code example calculates the age based on the date of birth:
        ///     <code><![CDATA[
        /// var dateOfBirth = new DateOnly(1980, 2, 29);
        /// int age = dateOfBirth.Age();
        /// Console.WriteLine(age); // The current age based on the date of birth.
        /// 
        /// age = dateOfBirth.Age(new DateOnly(2023, 2, 28));
        /// Console.WriteLine(age); // The age as of February 28, 2023.
        /// 
        /// var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        /// age = dateOfBirth.Age(timeZoneInfo: timeZone);
        /// Console.WriteLine(age); // The age based on the date of birth and a specific time zone.
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Age(DateOnly? maturityDate = null, TimeZoneInfo? timeZoneInfo = null)
        {
            timeZoneInfo ??= TimeZoneInfo.Local;

            // If maturityDate is null, compute 'today' from the specified TimeZoneInfo
            var today = maturityDate
                        ?? DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo));

            if (date > today)
            {
                throw new ArgumentException(
                    "Date of birth cannot be in the future relative to the maturity date or current date.",
                    nameof(date));
            }

            var age = today.Year - date.Year;

            // Adjust if the current month/day is before the birth month/day
            if (today.Month < date.Month ||
                (today.Month == date.Month && today.Day < date.Day))
            {
                age--;
            }

            return age;
        }

        /// <summary>
        ///     Calculates the age based on the date of birth, an optional <see cref="DateTime" /> maturity date,
        ///     and an optional <see cref="TimeZoneInfo" />.
        /// </summary>
        /// <param name="maturityDateTime">
        ///     An optional maturity date as a <see cref="DateTime" />.
        ///     If not provided (<c>null</c>), the current date in the specified time zone is used.
        /// </param>
        /// <param name="timeZoneInfo">
        ///     An optional time zone to determine the current date if <paramref name="maturityDateTime" /> is <c>null</c>.
        ///     If not provided, the local system time zone is used.
        /// </param>
        /// <returns>The calculated age in years.</returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if the source date (date of birth) is in the future relative to the maturity date or current date.
        /// </exception>
        /// <remarks>
        ///     This overload is useful if you already have a <see cref="DateTime" /> value for the maturity date,
        ///     such as data from a database or external source, without manually converting to <see cref="DateOnly" />.
        ///     If <paramref name="maturityDateTime" /> is <c>null</c>, the current date in the specified time zone is used.
        ///     If the <paramref name="timeZoneInfo" /> is <c>null</c>, the local system time zone is used.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// var dateOfBirth = new DateOnly(1980, 2, 29);
        /// DateTime? maturityDateTime = new DateTime(2023, 2, 28, 10, 0, 0, DateTimeKind.Utc);
        /// var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        /// int age = dateOfBirth.Age(maturityDateTime, timeZone);
        /// Console.WriteLine(age); // The age as of February 28, 2023 in PST.
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Age(DateTime? maturityDateTime, TimeZoneInfo? timeZoneInfo = null)
        {
            timeZoneInfo ??= TimeZoneInfo.Local;

            // If maturityDateTime is null, compute 'today' from the specified TimeZoneInfo
            DateOnly today;
            if (!maturityDateTime.HasValue)
            {
                var currentDateTimeInZone = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
                today = DateOnly.FromDateTime(currentDateTimeInZone);
            }
            else
            {
                // Convert provided DateTime into DateOnly
                var dateTimeInZone = TimeZoneInfo.ConvertTime(maturityDateTime.Value, timeZoneInfo);
                today = DateOnly.FromDateTime(dateTimeInZone);
            }

            if (date > today)
            {
                throw new ArgumentException(
                    "Date of birth cannot be in the future relative to the maturity date or current date.",
                    nameof(date));
            }

            var age = today.Year - date.Year;

            // Adjust if the current month/day is before the birth month/day
            if (today.Month < date.Month ||
                (today.Month == date.Month && today.Day < date.Day))
            {
                age--;
            }

            return age;
        }

        /// <summary>
        ///     Adds a specified number of weeks to a <see cref="DateOnly" /> object.
        /// </summary>
        /// <param name="numberOfWeeks">
        ///     The number of weeks to add, which can be fractional. Positive values add weeks; negative values subtract weeks.
        /// </param>
        /// <returns>
        ///     A <see cref="DateOnly" /> object that is the result of adding the specified number of weeks to the source date.
        /// </returns>
        /// <remarks>
        ///     This method converts the number of weeks to days by multiplying by 7 and then rounds the result to the nearest
        ///     whole number using <see cref="Math.Round(double, MidpointRounding)" /> with
        ///     <see cref="MidpointRounding.AwayFromZero" />.
        ///     The calculated number of days is then added to the source date using <see cref="DateOnly.AddDays(int)" />.
        ///     Fractional weeks are converted to days and rounded to the nearest whole day. Negative fractional weeks are handled
        ///     similarly.
        ///     Edge case: If the resulting day count moves the date outside the valid <see cref="DateOnly" /> range, an
        ///     <see cref="ArgumentOutOfRangeException" /> may be thrown by <see cref="DateOnly.AddDays(int)" />.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     DateOnly date = new DateOnly(2023, 1, 1);
        ///     DateOnly newDate = date.AddWeeks(2.5);
        ///     // newDate is January 19, 2023, as 2.5 weeks (17.5 days) rounds to 18 days
        ///     
        ///     newDate = date.AddWeeks(-1.5);
        ///     // newDate is December 21, 2022, as -1.5 weeks (-10.5 days) rounds to -11 days
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly AddWeeks(double numberOfWeeks)
        {
            if (numberOfWeeks == 0d)
            {
                return date;
            }

            var numberOfDaysToAdd = (int)Math.Round(numberOfWeeks * 7d, MidpointRounding.AwayFromZero);
            return date.AddDays(numberOfDaysToAdd);
        }

        /// <summary>
        ///     Gets the number of days in the month of the source <see cref="DateOnly" />.
        /// </summary>
        /// <returns>The number of days in the month of the source date.</returns>
        /// <remarks>
        ///     Uses <see cref="DateTime.DaysInMonth" /> to obtain the number of days in the month and year of the source date.
        ///     If the year is less than 1 or greater than 9999, or if the month is less than 1 or greater than 12,
        ///     an <see cref="ArgumentOutOfRangeException" /> is thrown by <see cref="DateTime.DaysInMonth" />.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the month component is less than 1 or greater than 12, or the year component is less than 1 or greater
        ///     than 9999.
        /// </exception>
        /// <example>
        ///     <code><![CDATA[
        ///     DateOnly date = new DateOnly(2023, 2, 15);
        ///     int days = date.DaysInMonth();
        ///     // days is 28, as February 2023 has 28 days
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DaysInMonth()
        {
            return DateTime.DaysInMonth(date.Year, date.Month);
        }

        /// <summary>
        ///     Gets the first date of the month for the source <see cref="DateOnly" />. Optionally, finds the first occurrence
        ///     of a specific <see cref="DayOfWeek" />.
        /// </summary>
        /// <param name="dayOfWeek">
        ///     Optional. The day of the week to find within the month. If <c>null</c>, the first day of the month is
        ///     returned.
        /// </param>
        /// <returns>
        ///     The first date of the month, or the first occurrence of the specified <see cref="DayOfWeek" /> within that month.
        /// </returns>
        /// <remarks>
        ///     Calculates the first date of the specified month from the source date. If a <see cref="DayOfWeek" /> is
        ///     provided, it calculates the offset to the first occurrence of that day within the month without iteration.
        ///     If <paramref name="dayOfWeek" /> matches the <see cref="DateOnly.DayOfWeek" /> of the first day of the month,
        ///     the 1st of the month is returned directly (offset = 0).
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     DateOnly date = new DateOnly(2023, 1, 15);
        ///     DateOnly firstDate = date.GetFirstDateOfMonth();
        ///     // firstDate is January 1, 2023
        /// 
        ///     DateOnly firstMonday = date.GetFirstDateOfMonth(DayOfWeek.Monday);
        ///     // firstMonday is January 2, 2023
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly GetFirstDateOfMonth(DayOfWeek? dayOfWeek = null)
        {
            var firstDateOfMonth = new DateOnly(date.Year, date.Month, 1);

            // If no specific dayOfWeek is requested, just return the first day of the month.
            if (!dayOfWeek.HasValue)
            {
                return firstDateOfMonth;
            }

            // Optional micro-optimization: short-circuit if they match
            if (dayOfWeek.Value == firstDateOfMonth.DayOfWeek)
            {
                return firstDateOfMonth;
            }

            var daysToAdd = ((int)dayOfWeek.Value - (int)firstDateOfMonth.DayOfWeek + 7) % 7;
            return firstDateOfMonth.AddDays(daysToAdd);
        }

        /// <summary>
        ///     Gets the first date of the week for the source <see cref="DateOnly" />, based on the specified or current
        ///     culture's first day of the week.
        /// </summary>
        /// <param name="cultureInfo">
        ///     Optional. The <see cref="CultureInfo" /> to determine the first day of the week.
        ///     If <c>null</c>, the current culture is used.
        /// </param>
        /// <returns>
        ///     A <see cref="DateOnly" /> object representing the first date of the week for the source date.
        /// </returns>
        /// <remarks>
        ///     Calculates the first date of the week based on the specified or current culture's definition of the
        ///     first day of the week. It computes the offset from the source date to the first day of the week without iteration.
        ///     Week calculations are culture-based and independent of time zones.
        ///     Edge case: If the source date is very close to <see cref="DateOnly.MinValue" /> and the calculated offset is large,
        ///     calling <see cref="DateOnly.AddDays(int)" /> with a negative value might throw an
        ///     <see cref="ArgumentOutOfRangeException" />.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     DateOnly date = new DateOnly(2023, 1, 18); // This is a Wednesday
        ///     DateOnly firstDateOfWeek = date.GetFirstDateOfWeek();
        ///     // firstDateOfWeek is Sunday, January 15, 2023, based on the current culture's first day of the week
        /// 
        ///     // Using a specific culture
        ///     CultureInfo germanCulture = new CultureInfo("de-DE");
        ///     firstDateOfWeek = date.GetFirstDateOfWeek(germanCulture);
        ///     // firstDateOfWeek is Monday, January 16, 2023, based on German culture's first day of the week
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private DateOnly GetFirstDateOfWeek(CultureInfo? cultureInfo = null)
        {
            var effectiveCulture = cultureInfo ?? CultureInfo.CurrentCulture;
            var firstDayOfWeek = effectiveCulture.DateTimeFormat.FirstDayOfWeek;

            var offset = ((int)date.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
            return offset == 0 ? date : date.AddDays(-offset);
        }

        /// <summary>
        ///     Gets the last date of the month for the source <see cref="DateOnly" />. Optionally, finds the last occurrence of
        ///     a specific <see cref="DayOfWeek" />.
        /// </summary>
        /// <param name="dayOfWeek">
        ///     Optional. The day of the week to find within the month. If <c>null</c>, the last day of the month is
        ///     returned.
        /// </param>
        /// <returns>
        ///     The last date of the month, or the last occurrence of the specified <see cref="DayOfWeek" /> within that month.
        /// </returns>
        /// <remarks>
        ///     Calculates the last date of the specified month from the source date. If a <see cref="DayOfWeek" /> is
        ///     provided, it calculates the offset to the last occurrence of that day within the month without iteration.
        ///     Edge case: If the source date is very close to <see cref="DateOnly.MinValue" /> and subtracting days pushes it
        ///     below the valid range, an <see cref="ArgumentOutOfRangeException" /> may occur.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     DateOnly date = new DateOnly(2023, 1, 15);
        ///     DateOnly lastDate = date.GetLastDateOfMonth();
        ///     // lastDate is January 31, 2023
        /// 
        ///     DateOnly lastFriday = date.GetLastDateOfMonth(DayOfWeek.Friday);
        ///     // lastFriday is January 27, 2023
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly GetLastDateOfMonth(DayOfWeek? dayOfWeek = null)
        {
            var daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);
            var lastDateOfMonth = new DateOnly(date.Year, date.Month, daysInMonth);

            if (!dayOfWeek.HasValue)
            {
                return lastDateOfMonth;
            }

            // Optional micro-optimization: if they match, no need to adjust
            if (lastDateOfMonth.DayOfWeek == dayOfWeek.Value)
            {
                return lastDateOfMonth;
            }

            var daysToSubtract = ((int)lastDateOfMonth.DayOfWeek - (int)dayOfWeek.Value + 7) % 7;
            return lastDateOfMonth.AddDays(-daysToSubtract);
        }

        /// <summary>
        ///     Gets the last date of the week for the source <see cref="DateOnly" /> value.
        /// </summary>
        /// <param name="cultureInfo">
        ///     The <see cref="CultureInfo" /> to use for determining the first day of the week; if
        ///     <c>null</c>, the current culture is used.
        /// </param>
        /// <returns>The last date of the week for the source <see cref="DateOnly" /> value.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly currentDate = DateOnly.Today;
        /// CultureInfo cultureInfo = new CultureInfo("en-US");
        /// DateOnly lastDateOfWeek = currentDate.GetLastDateOfWeek(cultureInfo);
        /// Console.WriteLine($"Last date of the week: {lastDateOfWeek}");
        /// ]]></code>
        /// </example>
        /// <remarks>
        ///     Uses the <see cref="GetFirstDateOfWeek" /> method to find the first date of the week and then adds 6
        ///     days to get the last date of the week. Week calculations are culture-based and independent of time zones.
        ///     If <paramref name="cultureInfo" /> is <c>null</c>, the <see cref="CultureInfo.CurrentCulture" /> is used to
        ///     determine the first day of the week.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly GetLastDateOfWeek(CultureInfo? cultureInfo = null)
        {
            return date.GetFirstDateOfWeek(cultureInfo).AddDays(6);
        }

        /// <summary>
        ///     Calculates the number of days between two <see cref="DateOnly" /> instances.
        /// </summary>
        /// <param name="toDate">The ending date for the calculation.</param>
        /// <returns>
        ///     The number of days between the two dates. The result can be positive or negative depending on the order of the
        ///     dates.
        /// </returns>
        /// <remarks>
        ///     Calculates the number of days between the source date and <paramref name="toDate" /> by subtracting
        ///     their <see cref="DateOnly.DayNumber" /> values. If <paramref name="toDate" /> is earlier than the source date,
        ///     the result will be negative. Use <c>Math.Abs</c> if you require the absolute number of days.
        /// </remarks>
        /// <example>
        ///     Calculating positive day difference:
        ///     <code><![CDATA[
        /// DateOnly fromDate = new(2023, 3, 1);
        /// DateOnly toDate = new(2023, 3, 28);
        /// int numberOfDays = fromDate.GetNumberOfDays(toDate); // numberOfDays will be 27
        /// ]]></code>
        ///     Calculating negative day difference:
        ///     <code><![CDATA[
        /// DateOnly fromDate = new(2023, 3, 28);
        /// DateOnly toDate = new(2023, 3, 1);
        /// int numberOfDays = fromDate.GetNumberOfDays(toDate); // numberOfDays will be -27
        /// ]]></code>
        ///     Using absolute value:
        ///     <code><![CDATA[
        /// DateOnly fromDate = new(2023, 3, 28);
        /// DateOnly toDate = new(2023, 3, 1);
        /// int numberOfDays = Math.Abs(fromDate.GetNumberOfDays(toDate)); // numberOfDays will be 27
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNumberOfDays(DateOnly toDate)
        {
            return toDate.DayNumber - date.DayNumber;
        }

        /// <summary>
        ///     Determines whether a <see cref="DateOnly" /> instance is after another <see cref="DateOnly" /> instance.
        /// </summary>
        /// <param name="other">The other date to compare against.</param>
        /// <returns>
        ///     <see langword="true" /> if the source date is after the other date; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Compares two <see cref="DateOnly" /> instances to determine whether the source date is after
        ///     the other date. It uses the <see cref="DateOnly.CompareTo(DateOnly)" /> method to perform the comparison.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly sourceDate = new(2023, 3, 28);
        /// DateOnly otherDate = new(2023, 3, 1);
        /// bool isAfter = sourceDate.IsAfter(otherDate); // isAfter will be true
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAfter(DateOnly other)
        {
            return date.CompareTo(other) > 0;
        }

        /// <summary>
        ///     Determines whether a <see cref="DateOnly" /> instance is before another <see cref="DateOnly" /> instance.
        /// </summary>
        /// <param name="other">The other date to compare against.</param>
        /// <returns>
        ///     <see langword="true" /> if the source date is before the other date; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Compares two <see cref="DateOnly" /> instances to determine whether the source date is before
        ///     the other date. It uses the <see cref="DateOnly.CompareTo(DateOnly)" /> method to perform the comparison.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly sourceDate = new(2023, 3, 1);
        /// DateOnly otherDate = new(2023, 3, 28);
        /// bool isBefore = sourceDate.IsBefore(otherDate); // isBefore will be true
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBefore(DateOnly other)
        {
            return date.CompareTo(other) < 0;
        }

        /// <summary>
        ///     Determines whether a <see cref="DateOnly" /> instance represents today's date.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the date is today; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Compares the source <see cref="DateOnly" /> instance to today's date to determine whether the instance
        ///     represents today's date. It uses <see cref="DateOnly.FromDateTime(DateTime)" /> with <see cref="DateTime.Today" />
        ///     to obtain today's date. This method uses the system's local time zone.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly date = new(2023, 3, 28);
        /// bool isToday = date.IsToday(); // Depends on the current date
        /// 
        /// DateOnly todayDate = DateOnly.FromDateTime(DateTime.Today);
        /// bool isToday = todayDate.IsToday(); // isToday will be true
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsToday()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return date == today;
        }

        /// <summary>
        ///     Determines whether a <see cref="DateOnly" /> instance represents a leap day.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the date is a leap day; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     A leap day is defined as February 29th. This method checks if the source date is February 29th.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly leapDay = new(2020, 2, 29);
        /// bool isLeapDay = leapDay.IsLeapDay(); // isLeapDay will be true
        /// 
        /// DateOnly nonLeapDay = new(2021, 2, 28);
        /// bool isLeapDay = nonLeapDay.IsLeapDay(); // isLeapDay will be false
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLeapDay()
        {
            return date is { Month: 2, Day: 29 };
        }

        /// <summary>
        ///     Determines whether the year of the source <see cref="DateOnly" /> instance is a leap year.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the year is a leap year; otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     Uses <see cref="DateTime.IsLeapYear(int)" /> to determine if the year is a leap year.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// DateOnly dateInLeapYear = new(2020, 3, 1);
        /// bool isLeapYear = dateInLeapYear.IsLeapYear(); // isLeapYear will be true
        /// 
        /// DateOnly dateInNonLeapYear = new(2021, 3, 1);
        /// bool isLeapYear = dateInNonLeapYear.IsLeapYear(); // isLeapYear will be false
        ///     ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLeapYear()
        {
            return DateTime.IsLeapYear(date.Year);
        }
    }
}