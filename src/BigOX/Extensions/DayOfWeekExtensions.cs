using BigOX.Validation;

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of useful extension methods for working with <see cref="DayOfWeek" /> enums.
/// </summary>
public static class DayOfWeekExtensions
{
    /// <summary>
    ///     Provides extension methods for the <see cref="DayOfWeek" /> enum.
    /// </summary>
    /// <param name="dayOfWeek">The <see cref="DayOfWeek" /> value to add days to.</param>
    extension(DayOfWeek dayOfWeek)
    {
        /// <summary>
        ///     Adds a specified number of days to the current <see cref="DayOfWeek" /> value, cycling through the days of the
        ///     week.
        /// </summary>
        /// <param name="numberOfDays">
        ///     The number of days to add. Can be negative to subtract days. The result will wrap around if it exceeds
        ///     the boundaries of the <see cref="DayOfWeek" /> enum (Sunday to Saturday).
        /// </param>
        /// <returns>
        ///     A <see cref="DayOfWeek" /> value that is <paramref name="numberOfDays" /> days from the
        ///     source parameter.
        /// </returns>
        /// <remarks>
        ///     This method allows the addition or subtraction of days from a <see cref="DayOfWeek" /> value.
        ///     Negative values of <paramref name="numberOfDays" /> will subtract days, and positive values will add days.
        ///     The result is always normalized within the range of the <see cref="DayOfWeek" /> enum (0 = Sunday to 6 = Saturday).
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     DayOfWeek monday = DayOfWeek.Monday;
        ///     DayOfWeek wednesday = monday.AddDays(2); // Adding 2 days to Monday
        ///     DayOfWeek sunday = monday.AddDays(-1);   // Subtracting 1 day from Monday
        ///     Console.WriteLine(wednesday);            // Output: Wednesday
        ///     Console.WriteLine(sunday);               // Output: Sunday
        ///     ]]></code>
        /// </example>
        public DayOfWeek AddDays(int numberOfDays = 1)
        {
            var totalDays = (int)dayOfWeek + numberOfDays;
            var normalizedDays = (totalDays % 7 + 7) % 7; // Handles negative values correctly
            return (DayOfWeek)normalizedDays;
        }

        /// <summary>
        ///     Generates a sequence of the next specified number of days of the week, starting from the provided
        ///     <paramref name="dayOfWeek" />.
        /// </summary>
        /// <param name="count">
        ///     The number of days to generate, starting from <paramref name="dayOfWeek" />.
        ///     Defaults to 7, representing a full week. Must be 1 or greater.
        /// </param>
        /// <returns>
        ///     An <see cref="IEnumerable{DayOfWeek}" /> representing the next specified number of days of the week,
        ///     cycling through the week as needed (Sunday to Saturday).
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="count" /> is less than or equal to 0.
        /// </exception>
        /// <remarks>
        ///     This method returns a sequence of <see cref="DayOfWeek" /> values, beginning with <paramref name="dayOfWeek" /> and
        ///     including the next <paramref name="count" /> days, wrapping around if necessary (e.g., after Saturday, it returns
        ///     Sunday).
        /// </remarks>
        /// <example>
        ///     <code><![CDATA[
        ///     // Generates the next 3 days starting from Monday.
        ///     IEnumerable<DayOfWeek> days = DayOfWeek.Monday.GetNextDays(3);
        ///     // Result: Monday, Tuesday, Wednesday
        /// 
        ///     // Generates a full week starting from Thursday.
        ///     IEnumerable<DayOfWeek> week = DayOfWeek.Thursday.GetNextDays();
        ///     // Result: Thursday, Friday, Saturday, Sunday, Monday, Tuesday, Wednesday
        ///     ]]></code>
        /// </example>
        public IEnumerable<DayOfWeek> GetNextDays(int count = 7)
        {
            Guard.Minimum(count, 1);

            var start = (int)dayOfWeek;
            for (var offset = 0; offset < count; offset++)
            {
                yield return (DayOfWeek)((start + offset) % 7);
            }
        }
    }
}