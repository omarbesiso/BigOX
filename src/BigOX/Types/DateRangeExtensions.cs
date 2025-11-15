using System.Runtime.CompilerServices;
using BigOX.Validation;

namespace BigOX.Types;

/// <summary>
///     Extensions for <see cref="DateRange" /> to provide additional functionality.
/// </summary>
public static class DateRangeExtensions
{
    /// <summary>
    ///     Returns the minimum of two end dates, treating null as infinity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static DateOnly? EndMin(DateOnly? a, DateOnly? b)
    {
        if (a is null)
        {
            return b; // min(∞, b) = b (even if b is null => null)
        }

        if (b is null)
        {
            return a; // min(a, ∞) = a
        }

        return a < b ? a : b;
    }

    /// <summary>
    ///     Extension methods for <see cref="DateRange" />.
    /// </summary>
    /// <param name="dateRange">The date range to extend.</param>
    extension(in DateRange dateRange)
    {
        /// <summary>
        ///     Returns the inclusive number of days in a finite range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Duration()
        {
            var end = dateRange.EndDate ??
                      throw new InvalidOperationException("Open-ended range has no finite duration.");
            return end.DayNumber - dateRange.StartDate.DayNumber + 1;
        }

        /// <summary>
        ///     Attempts to get the inclusive duration in days for a finite range.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDuration(out int days)
        {
            var end = dateRange.EndDate;
            if (!end.HasValue)
            {
                days = 0;
                return false;
            }

            days = end.Value.DayNumber - dateRange.StartDate.DayNumber + 1;
            return true;
        }

        /// <summary>
        ///     Returns <see langword="true" /> if <paramref name="date" /> lies within <paramref name="dateRange" /> (inclusive).
        /// </summary>
        /// <remarks>
        ///     <c>default(DateRange)</c> is equivalent to <c>DateOnly.MinValue|∞</c>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(DateOnly date)
        {
            // Use local copies to avoid repeated field access (JIT will likely inline anyway).
            var start = dateRange.StartDate;
            var end = dateRange.EffectiveEnd;
            return (date.DayNumber >= start.DayNumber) &
                   (date.DayNumber <= end.DayNumber); // use single ampersand to avoid branchy JIT expansion
        }

        /// <summary>
        ///     Determines whether two <see cref="DateRange" /> values overlap.
        ///     Ranges overlap iff max(startA, startB) ≤ min(endA, endB).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in DateRange other)
        {
            var start = dateRange.StartDate.DayNumber >= other.StartDate.DayNumber
                ? dateRange.StartDate.DayNumber
                : other.StartDate.DayNumber;
            var end = dateRange.EffectiveEnd.DayNumber <= other.EffectiveEnd.DayNumber
                ? dateRange.EffectiveEnd.DayNumber
                : other.EffectiveEnd.DayNumber;
            return start <= end;
        }

        /// <summary>
        ///     Computes the intersection of two ranges. Returns null if there is no overlap.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DateRange? Intersection(in DateRange right)
        {
            if (!dateRange.Overlaps(right))
            {
                return null;
            }

            var start = dateRange.StartDate.DayNumber >= right.StartDate.DayNumber
                ? dateRange.StartDate
                : right.StartDate;
            var end = EndMin(dateRange.EndDate, right.EndDate);
            return new DateRange(start, end);
        }
    }

    /// <summary>
    ///     Extension methods for <see cref="DateRange" />.
    /// </summary>
    extension(DateRange dateRange)
    {
        /// <summary>
        ///     Enumerates the range as contiguous 7‑day chunks anchored at <see cref="DateRange.StartDate" />.
        ///     Each yielded <see cref="DateRange" /> is inclusive; the final chunk may be shorter than 7 days.
        ///     Does not align to calendar/ISO weeks (strictly start..start+6 stepping by 7).
        /// </summary>
        public IEnumerable<DateRange> GetWeeksInRange(int? maxWeeks = null)
        {
            var isOpenEnded = dateRange.IsOpenEnded;

            if (isOpenEnded)
            {
                Guard.NotNull(maxWeeks, "Open-ended range requires maxWeeks to avoid unbounded enumeration.");
                Guard.Positive(maxWeeks.Value, nameof(maxWeeks),
                    "For open-ended ranges, maxWeeks must be greater than zero.");
            }
            else if (maxWeeks is < 0)
            {
                Guard.NonNegative(maxWeeks.Value, nameof(maxWeeks), "For closed ranges, maxWeeks cannot be negative.");
            }

            var cap = maxWeeks ?? int.MaxValue;
            if (cap == 0)
            {
                yield break;
            }

            var endInclusive = dateRange.EffectiveEnd;
            var start = dateRange.StartDate;
            var emitted = 0;

            // Drive loop by DayNumber for safety near MaxSupportedDate.
            var endDay = endInclusive.DayNumber;
            var startDay = start.DayNumber;
            while (startDay <= endDay && emitted < cap)
            {
                var daysLeft = endDay - startDay; // >= 0
                var span = daysLeft >= 6 ? 6 : daysLeft; // 0..6
                var chunkEndDay = startDay + span;
                var chunkEnd = DateOnly.FromDayNumber(chunkEndDay);

                var outEnd = isOpenEnded && chunkEndDay == DateRange.MaxSupportedDate.DayNumber
                    ? (DateOnly?)null
                    : chunkEnd;

                yield return new DateRange(DateOnly.FromDayNumber(startDay), outEnd);
                emitted++;
                startDay += 7;
            }
        }

        /// <summary>
        ///     Lazily enumerates each day in the range (inclusive).
        /// </summary>
        public IEnumerable<DateOnly> EnumerateDays(int? maxCount = null)
        {
            if (maxCount.HasValue)
            {
                Guard.Minimum(maxCount.Value, 0, nameof(maxCount), "maxCount must be >= 0.");
            }

            if (maxCount == 0)
            {
                yield break;
            }

            var start = dateRange.StartDate;
            var end = dateRange.EffectiveEnd;
            if (start > end)
            {
                yield break; // defensive only
            }

            var startDay = start.DayNumber;
            var lastDay = end.DayNumber;

            if (maxCount is { } limit and > 0)
            {
                var target = (long)startDay + limit - 1L;
                if (target < lastDay)
                {
                    lastDay = (int)target;
                }
            }

            for (var day = startDay; day <= lastDay; day++)
            {
                yield return DateOnly.FromDayNumber(day);
            }
        }
    }
}