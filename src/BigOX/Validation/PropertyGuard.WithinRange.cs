using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property value is within the inclusive range defined by
    ///     <paramref name="minValue" /> and <paramref name="maxValue" />.
    /// </summary>
    /// <typeparam name="T">A comparable type that implements <see cref="IComparable{T}" />.</typeparam>
    /// <param name="value">The property value to validate.</param>
    /// <param name="minValue">The inclusive minimum allowable value.</param>
    /// <param name="maxValue">The inclusive maximum allowable value.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, captured automatically via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message. When omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it lies within the specified range.
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
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : IComparable<T>
    {
        return Guard.WithinRange(value, minValue, maxValue, propertyName, exceptionMessage);
    }
}