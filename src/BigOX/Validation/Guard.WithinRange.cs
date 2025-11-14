using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that <paramref name="value" /> is within the inclusive range defined by
    ///     <paramref name="minValue" /> and <paramref name="maxValue" />.
    /// </summary>
    /// <typeparam name="T">A comparable type that implements <see cref="IComparable{T}" />.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="minValue">The inclusive minimum allowable value.</param>
    /// <param name="maxValue">The inclusive maximum allowable value.</param>
    /// <param name="paramName">
    ///     Name of the argument being validated, captured automatically by
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message. When omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it lies between <paramref name="minValue" /> and
    ///     <paramref name="maxValue" />, inclusive.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="minValue" /> is greater than <paramref name="maxValue" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value" /> is outside the specified range.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T WithinRange<T>(
        T value,
        T minValue,
        T maxValue,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : IComparable<T>
    {
        // Validate the range parameters themselves.
        if (minValue.CompareTo(maxValue) > 0)
        {
            ThrowHelper.ThrowArgument(
                nameof(minValue),
                "The minimum value specified cannot be greater than the maximum value specified.");
        }

        // Fast-path success when inside range.
        if (value.CompareTo(minValue) >= 0 && value.CompareTo(maxValue) <= 0)
        {
            return value;
        }

        // Construct error and throw for out-of-range arguments.
        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must be between '{minValue}' and '{maxValue}'."
            : exceptionMessage;

        ThrowHelper.ThrowArgumentOutOfRange(paramName, value, message);
        return value; // unreachable – satisfies compiler flow analysis
    }
}