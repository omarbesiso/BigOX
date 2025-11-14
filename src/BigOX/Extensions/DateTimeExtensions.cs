using System.Globalization;
using System.Runtime.CompilerServices;
using BigOX.Validation;

// ReSharper disable MemberCanBePrivate.Global

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="DateTime" /> objects.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    ///     Returns the number of days in the specified year according to the specified calendar culture or the current culture
    ///     if none is specified.
    /// </summary>
    /// <param name="year">The year to evaluate. Must be in the range 1 through 9999.</param>
    /// <param name="cultureInfo">
    ///     Optional. The culture whose calendar is used to compute the days in the year. If <c>null</c>,
    ///     <see cref="CultureInfo.CurrentCulture" /> is used.
    /// </param>
    /// <returns>The number of days in the specified <paramref name="year" /> for the provided culture's calendar.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="year" /> is less than 1 or greater than 9999.
    /// </exception>
    /// <remarks>
    ///     This implementation uses <see cref="Calendar.GetDaysInYear(int,int)" /> with the correct era resolved from the
    ///     calendar. This avoids creating additional <see cref="DateTime" /> instances and ensures accuracy for calendars with
    ///     multiple eras.
    /// </remarks>
    /// <example>
    ///     <code><![CDATA[
    /// int year = 2024;
    /// int days = DateTimeExtensions.GetNumberOfDaysInYear(year);
    /// // days is 366
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetNumberOfDaysInYear(int year, CultureInfo? cultureInfo = null)
    {
        if ((uint)(year - 1) > 9998u)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        var cal = (cultureInfo ?? CultureInfo.CurrentCulture).Calendar;
        var era = cal.GetEra(new DateTime(year, 1, 1, cal));
        return cal.GetDaysInYear(year, era);
    }

    /// <param name="dateTime">
    ///     The <see cref="DateTime" /> instance the extension methods operate on.
    /// </param>
    extension(DateTime dateTime)
    {
        /// <summary>
        ///     Converts a <see cref="DateTime" /> to a <see cref="DateOnly" /> containing only the date component.
        /// </summary>
        /// <returns>A <see cref="DateOnly" /> with the year, month, and day from source.</returns>
        /// <remarks>
        ///     The time-of-day and <see cref="DateTime.Kind" /> are not represented in the resulting <see cref="DateOnly" />.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// DateTime dt = new(2023, 1, 15, 10, 30, 0);
        /// DateOnly d = dt.ToDateOnly();
        /// // d = 2023-01-15
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateOnly ToDateOnly()
        {
            return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
        }

        /// <summary>
        ///     Converts a <see cref="DateTime" /> to a <see cref="TimeOnly" /> containing only the time component.
        /// </summary>
        /// <returns>
        ///     A <see cref="TimeOnly" /> with the hour, minute, second, millisecond, and microsecond from
        ///     source.
        /// </returns>
        /// <example>
        ///     <code><![CDATA[
        /// DateTime dt = new(2023, 1, 15, 10, 30, 0, 500);
        /// TimeOnly t = dt.ToTimeOnly();
        /// // t = 10:30:00.5000000
        /// ]]></code>
        /// </example>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeOnly ToTimeOnly()
        {
            return new TimeOnly(dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond,
                dateTime.Microsecond);
        }

        /// <summary>
        ///     Calculates the age in full years using the date part of the birth date and the provided or current date.
        /// </summary>
        /// <param name="maturityDate">
        ///     Optional. The reference date to calculate age against. Defaults to "today" in the specified
        ///     time zone.
        /// </param>
        /// <param name="timeZoneInfo">
        ///     Optional. The time zone used to determine "today" when <paramref name="maturityDate" /> is
        ///     <c>null</c>. Defaults to <see cref="TimeZoneInfo.Local" />.
        /// </param>
        /// <returns>The age in full years.</returns>
        /// <exception cref="ArgumentException">Thrown when the birth date (source) is in the future.</exception>
        /// <remarks>
        ///     The calculation ignores the time-of-day component and subtracts one year if the birthday for the current year has
        ///     not yet occurred on the reference date.
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        /// var dob = new DateTime(1980, 1, 1);
        /// int age = dob.Age();
        /// ]]></code>
        /// </example>
        public int Age(DateTime? maturityDate = null, TimeZoneInfo? timeZoneInfo = null)
        {
            var nowLocal = DateTime.Now; // single read
            Guard.Requires(dateTime, d => d <= nowLocal, "Date of birth cannot be in the future.");

            var today = maturityDate ??
                        TimeZoneInfo.ConvertTime(DateTime.UtcNow, timeZoneInfo ?? TimeZoneInfo.Local);

            var age = today.Year - dateTime.Year;
            if (today.Month < dateTime.Month || (today.Month == dateTime.Month && today.Day < dateTime.Day))
            {
                age--;
            }

            return age;
        }

        /// <summary>
        ///     Adds a number of weeks to the date, rounding partial weeks up to the next whole day.
        /// </summary>
        /// <param name="numberOfWeeks">The number of weeks to add. Can be fractional.</param>
        /// <returns>A <see cref="DateTime" /> incremented by the specified number of weeks.</returns>
        /// <remarks>
        ///     Internally multiplies by 7 then rounds away from zero to the nearest whole day before calling
        ///     <see cref="DateTime.AddDays(double)" />. This ensures that any fractional part always pushes to the next day in
        ///     the direction of travel (future for positive, past for negative).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime AddWeeks(double numberOfWeeks)
        {
            var days = numberOfWeeks * 7.0;
            var roundedDays = days >= 0 ? Math.Ceiling(days) : Math.Floor(days);
            return dateTime.AddDays(roundedDays);
        }

        /// <summary>
        ///     Gets the number of days in the month of the given date.
        /// </summary>
        /// <returns>The number of days in the month of source.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DaysInMonth()
        {
            return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
        }

        /// <summary>
        ///     Gets the first date of the month, or the first occurrence of a specific <see cref="DayOfWeek" /> in that month.
        /// </summary>
        /// <param name="dayOfWeek">
        ///     Optional. The desired <see cref="DayOfWeek" />. If <c>null</c>, the first calendar day of the
        ///     month is returned.
        /// </param>
        /// <returns>The first day of the month or the first occurrence of <paramref name="dayOfWeek" />.</returns>
        /// <remarks>
        ///     Uses arithmetic offset from the first day to avoid iterative scanning of days.
        /// </remarks>
        public DateTime GetFirstDateOfMonth(DayOfWeek? dayOfWeek = null)
        {
            var first = new DateTime(dateTime.Year, dateTime.Month, 1);
            if (!dayOfWeek.HasValue)
            {
                return first;
            }

            var offset = ((int)dayOfWeek.Value - (int)first.DayOfWeek + 7) % 7;
            return first.AddDays(offset);
        }

        /// <summary>
        ///     Gets the first date of the week for the specified culture.
        /// </summary>
        /// <param name="cultureInfo">
        ///     Optional. The culture used to determine the first day of the week. Defaults to
        ///     <see cref="CultureInfo.CurrentCulture" />.
        /// </param>
        /// <returns>The date representing the first day of the week containing source.</returns>
        /// <remarks>
        ///     Computes the offset based on <see cref="DateTime.DayOfWeek" /> and the culture's
        ///     <see cref="DateTimeFormatInfo.FirstDayOfWeek" />.
        ///     The original source value is not modified.
        /// </remarks>
        public DateTime GetFirstDateOfWeek(CultureInfo? cultureInfo = null)
        {
            var firstDayOfWeek = (cultureInfo ?? CultureInfo.CurrentCulture).DateTimeFormat.FirstDayOfWeek;
            var diff = ((int)dateTime.DayOfWeek - (int)firstDayOfWeek + 7) % 7;
            return dateTime.AddDays(-diff);
        }

        /// <summary>
        ///     Gets the last date of the month, or the last occurrence of a specific <see cref="DayOfWeek" /> in that month.
        /// </summary>
        /// <param name="dayOfWeek">
        ///     Optional. The desired <see cref="DayOfWeek" />. If <c>null</c>, the last calendar day of the
        ///     month is returned.
        /// </param>
        /// <returns>The last day of the month or the last occurrence of <paramref name="dayOfWeek" />.</returns>
        /// <remarks>
        ///     Uses arithmetic offset from the last day to avoid iterative scanning of days.
        /// </remarks>
        public DateTime GetLastDateOfMonth(DayOfWeek? dayOfWeek = null)
        {
            var dim = DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            var last = new DateTime(dateTime.Year, dateTime.Month, dim);
            if (!dayOfWeek.HasValue)
            {
                return last;
            }

            var offset = ((int)last.DayOfWeek - (int)dayOfWeek.Value + 7) % 7;
            return last.AddDays(-offset);
        }

        /// <summary>
        ///     Gets the last date of the week for the specified culture.
        /// </summary>
        /// <param name="cultureInfo">
        ///     Optional. The culture used to determine the first day of the week. Defaults to
        ///     <see cref="CultureInfo.CurrentCulture" />.
        /// </param>
        /// <returns>The date representing the last day of the week containing source.</returns>
        /// <remarks>
        ///     Computed as <c>GetFirstDateOfWeek(cultureInfo).AddDays(6)</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime GetLastDateOfWeek(CultureInfo? cultureInfo = null)
        {
            return dateTime.GetFirstDateOfWeek(cultureInfo).AddDays(6);
        }

        /// <summary>
        ///     Calculates the number of whole days between this date and another date.
        /// </summary>
        /// <param name="toDate">The end date.</param>
        /// <returns>The number of days between source and <paramref name="toDate" />.</returns>
        /// <remarks>
        ///     The calculation uses the <see cref="DateTime.Date" /> part of both values and returns a signed integer which can be
        ///     negative.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetNumberOfDays(DateTime toDate)
        {
            return (toDate.Date - dateTime.Date).Days;
        }

        /// <summary>
        ///     Determines whether this date is after another date.
        /// </summary>
        /// <param name="other">The date to compare against.</param>
        /// <returns><c>true</c> if source is later than <paramref name="other" />; otherwise, <c>false</c>.</returns>
        /// <remarks>Comparison uses the default <see cref="DateTime" /> ordering.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAfter(DateTime other)
        {
            return dateTime > other;
        }

        /// <summary>
        ///     Determines whether this date is before another date.
        /// </summary>
        /// <param name="other">The date to compare against.</param>
        /// <returns>
        ///     <c>true</c> if source is earlier than <paramref name="other" />; otherwise, <c>false</c>
        ///     .
        /// </returns>
        /// <remarks>Comparison uses the default <see cref="DateTime" /> ordering.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBefore(DateTime other)
        {
            return dateTime < other;
        }

        /// <summary>
        ///     Determines whether this date is within the specified range.
        /// </summary>
        /// <param name="rangeBeg">
        ///     The inclusive lower bound when <paramref name="isInclusive" /> is <c>true</c>, otherwise the
        ///     exclusive lower bound.
        /// </param>
        /// <param name="rangeEnd">
        ///     The inclusive upper bound when <paramref name="isInclusive" /> is <c>true</c>, otherwise the
        ///     exclusive upper bound.
        /// </param>
        /// <param name="isInclusive">If <c>true</c>, bounds are inclusive; otherwise, exclusive.</param>
        /// <returns><c>true</c> if source falls within the specified range; otherwise, <c>false</c>.</returns>
        /// <example>
        ///     <code><![CDATA[
        /// var dt = new DateTime(2023, 1, 15);
        /// var start = new DateTime(2023, 1, 1);
        /// var end = new DateTime(2023, 1, 31);
        /// bool within = dt.IsBetween(start, end); // true
        /// ]]></code>
        /// </example>
        public bool IsBetween(DateTime rangeBeg, DateTime rangeEnd, bool isInclusive = true)
        {
            var ticks = dateTime.Ticks;
            var beg = rangeBeg.Ticks;
            var end = rangeEnd.Ticks;
            // Normalize bounds to support inverted ranges
            var min = Math.Min(beg, end);
            var max = Math.Max(beg, end);
            return isInclusive ? ticks >= min && ticks <= max : ticks > min && ticks < max;
        }

        /// <summary>
        ///     Compares only the date components (year, month, day) of two <see cref="DateTime" /> values for equality.
        /// </summary>
        /// <param name="dateToCompare">The date to compare to.</param>
        /// <returns><c>true</c> if the date components are equal; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDateEqual(DateTime dateToCompare)
        {
            return dateTime.Date == dateToCompare.Date;
        }

        /// <summary>
        ///     Compares only the time-of-day components of two <see cref="DateTime" /> values for equality.
        /// </summary>
        /// <param name="timeToCompare">The date whose time component is compared.</param>
        /// <returns><c>true</c> if the time components are equal; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTimeEqual(DateTime timeToCompare)
        {
            return dateTime.TimeOfDay == timeToCompare.TimeOfDay;
        }

        /// <summary>
        ///     Determines whether the date component is equal to today's date in the local time zone.
        /// </summary>
        /// <returns><c>true</c> if source is today; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsToday()
        {
            return dateTime.Date == DateTime.Today;
        }
    }

    /// <param name="dateTime">The dateTime to check.</param>
    extension(DateTime dateTime)
    {
        /// <summary>
        ///     Determines whether a <see cref="DateTime" /> instance represents the leap day (February 29th).
        /// </summary>
        /// <returns><c>true</c> if the date is February 29th; otherwise, <c>false</c>.</returns>
        /// <remarks>
        ///     This method checks day and month only. Use <see cref="DateTime.IsLeapYear(int)" /> on <see cref="DateTime.Year" />
        ///     if you also need to verify the year is a leap year.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLeapDay()
        {
            return dateTime.Month == 2 && dateTime.Day == 29;
        }

        /// <summary>
        ///     Returns the elapsed time between this date/time and <see cref="DateTime.Now" />.
        /// </summary>
        /// <returns>A <see cref="TimeSpan" /> representing the elapsed interval.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan Elapsed()
        {
            return DateTime.Now - dateTime;
        }

        /// <summary>
        ///     Creates a new <see cref="DateTime" /> with the same date and the provided <see cref="TimeSpan" /> as its
        ///     time-of-day.
        /// </summary>
        /// <param name="time">The time-of-day to apply. Must be in the range [00:00:00, 24:00:00).</param>
        /// <returns>A new <see cref="DateTime" /> with the same date and the specified time.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="time" /> is negative or ≥ 24 hours.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime SetTime(TimeSpan time)
        {
            return dateTime.Date + time;
        }

        /// <summary>
        ///     Creates a new <see cref="DateTime" /> with the same date and the provided components as its time-of-day.
        /// </summary>
        /// <param name="hours">The hours component (0-23).</param>
        /// <param name="minutes">The minutes component (0-59).</param>
        /// <param name="seconds">The seconds component (0-59).</param>
        /// <param name="milliseconds">The milliseconds component (0-999).</param>
        /// <returns>A new <see cref="DateTime" /> with the specified time-of-day.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when any component is outside its valid range.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime SetTime(int hours = 0, int minutes = 0, int seconds = 0, int milliseconds = 0)
        {
            return dateTime.Date.Add(new TimeSpan(0, hours, minutes, seconds, milliseconds));
        }

        /// <summary>
        ///     Creates a new <see cref="DateTime" /> with the same date and the provided <see cref="TimeOnly" /> as its
        ///     time-of-day.
        /// </summary>
        /// <param name="timeOnly">The <see cref="TimeOnly" /> value to apply.</param>
        /// <returns>A new <see cref="DateTime" /> with the specified time-of-day.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime SetTime(TimeOnly timeOnly)
        {
            return dateTime.Date + timeOnly.ToTimeSpan();
        }

        /// <summary>
        ///     Returns a new instance representing the calendar day after this one.
        /// </summary>
        /// <returns>The next calendar day.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime NextDay()
        {
            return dateTime.AddDays(1);
        }

        /// <summary>
        ///     Returns a new instance representing the calendar day before this one.
        /// </summary>
        /// <returns>The previous calendar day.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateTime PreviousDay()
        {
            return dateTime.AddDays(-1);
        }

        /// <summary>
        ///     Formats the value using the ISO 8601 round-trip format specifier.
        /// </summary>
        /// <returns>An ISO 8601 string representation of the value.</returns>
        /// <remarks>
        ///     Uses the standard format string "O" with <see cref="CultureInfo.InvariantCulture" /> to ensure consistent output.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetTimestamp()
        {
            return dateTime.ToString("O", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Enumerates the inclusive range of dates between this date and the specified end date.
        /// </summary>
        /// <param name="toDate">The end date of the range.</param>
        /// <returns>An <see cref="IEnumerable{T}" /> of <see cref="DateTime" /> values for each day in the range.</returns>
        /// <remarks>
        ///     The sequence is emitted in ascending or descending order depending on the relative ordering of the endpoints.
        /// </remarks>
        public IEnumerable<DateTime> GetDatesInRange(DateTime toDate)
        {
            if (dateTime == toDate)
            {
                yield return dateTime;
                yield break;
            }

            var step = dateTime < toDate ? 1 : -1;
            for (var dt = dateTime; dt != toDate; dt = dt.AddDays(step))
            {
                yield return dt;
            }

            yield return toDate;
        }
    }
}