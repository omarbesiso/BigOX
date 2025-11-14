using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a property value does not exceed a specified maximum.
    /// </summary>
    /// <typeparam name="T">A comparable type that implements <see cref="IComparable{T}" />.</typeparam>
    /// <param name="value">The property value to validate.</param>
    /// <param name="maxValue">The maximum allowable value.</param>
    /// <param name="propertyName">
    ///     Name of the property being validated, captured automatically via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
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
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : IComparable<T>
    {
        return Guard.Maximum(value, maxValue, propertyName, exceptionMessage);
    }
}