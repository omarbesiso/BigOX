using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that <paramref name="value" /> is not less than <paramref name="minValue" />.
    /// </summary>
    /// <typeparam name="T">A comparable type that implements <see cref="IComparable{T}" />.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="minValue">The minimum allowable value.</param>
    /// <param name="paramName">
    ///     Name of the argument being validated, captured automatically by
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message. When omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it is greater than or equal to <paramref name="minValue" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value" /> is less than <paramref name="minValue" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Minimum<T>(
        T value,
        T minValue,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(minValue) < 0)
        {
            var message = string.IsNullOrWhiteSpace(exceptionMessage)
                ? $"The value of '{paramName}' cannot be less than {minValue}."
                : exceptionMessage;

            ThrowHelper.ThrowArgumentOutOfRange(paramName, value, message);
        }

        return value;
    }
}