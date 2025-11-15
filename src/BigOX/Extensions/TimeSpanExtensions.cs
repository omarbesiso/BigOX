using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBePrivate.Global

namespace BigOX.Extensions;

/// <summary>
///     Provides a set of utility extension methods for the <see cref="TimeSpan" /> structure.
/// </summary>
public static class TimeSpanExtensions
{
    /// <summary>
    ///     Provides extension methods for <see cref="TimeSpan" />.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan" /> to convert.</param>
    extension(TimeSpan timeSpan)
    {
        /// <summary>
        ///     Converts a <see cref="TimeSpan" /> to a <see cref="TimeOnly" /> instance, wrapping around if the duration exceeds
        ///     24 hours.
        /// </summary>
        /// <returns>
        ///     A <see cref="TimeOnly" /> instance representing the time of day equivalent to the timespan
        ///     modulo 24 hours.
        /// </returns>
        /// <remarks>
        ///     This method normalizes the <see cref="TimeSpan" /> to ensure it is within a day's duration.
        ///     If the timespan is negative, it will wrap around to a positive time of day.
        /// </remarks>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public TimeOnly ToTimeOnly()
        {
            // Normalize the ticks within a 24-hour period
            var normalizedTicks = timeSpan.Ticks % TimeSpan.TicksPerDay;
            if (normalizedTicks < 0)
            {
                normalizedTicks += TimeSpan.TicksPerDay;
            }

            return TimeOnly.FromTimeSpan(TimeSpan.FromTicks(normalizedTicks));
        }
    }

    /// <summary>
    ///     Provides extension methods for nullable <see cref="TimeSpan" />.
    /// </summary>
    /// <param name="timeSpan">The nullable <see cref="TimeSpan" /> to convert.</param>
    extension(TimeSpan? timeSpan)
    {
        /// <summary>
        ///     Converts a nullable <see cref="TimeSpan" /> to a nullable <see cref="TimeOnly" /> instance,
        ///     wrapping around if the duration exceeds 24 hours.
        /// </summary>
        /// <returns>
        ///     A nullable <see cref="TimeOnly" /> instance representing the time of day equivalent to the
        ///     timespan modulo 24 hours, or <c>null</c> if the timespan is <c>null</c>.
        /// </returns>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeOnly? ToTimeOnly()
        {
            return timeSpan?.ToTimeOnly();
        }
    }
}