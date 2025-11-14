using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that <paramref name="value" /> does not exceed <paramref name="maxValue" />.
    /// </summary>
    /// <typeparam name="T">A comparable type that implements <see cref="IComparable{T}" />.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="maxValue">The maximum allowable value.</param>
    /// <param name="paramName">
    ///     Name of the argument being validated, captured automatically by
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message. When omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it is less than or equal to <paramref name="maxValue" />.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when <paramref name="value" /> is greater than <paramref name="maxValue" />.
    /// </exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Maximum<T>(
        T value,
        T maxValue,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(maxValue) <= 0)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' cannot exceed {maxValue}."
            : exceptionMessage;

        ThrowHelper.ThrowArgumentOutOfRange(paramName, value, message);

        return value;
    }
}