using System.Numerics;
using System.Runtime.CompilerServices;

// ReSharper disable UnusedMember.Global

namespace BigOX.Validation;

/// <summary>
///     Guard helpers specialised for property setters and initialisers.
/// </summary>
public static partial class PropertyGuard
{
    /// <summary>
    ///     Ensures that a numeric value is positive (greater than zero).
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="propertyName">The name of the property. Automatically supplied by the compiler.</param>
    /// <param name="exceptionMessage">An optional custom exception message.</param>
    /// <returns>The original value if the check passes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    /// <remarks>
    ///     This method is intended for use in property setters and initializers.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Positive<T>(
        T value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
    {
        return Guard.Positive(value, propertyName, exceptionMessage);
    }

    /// <summary>
    ///     Ensures that a numeric value is non-negative (zero or greater).
    /// </summary>
    /// <typeparam name="T">The numeric type.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="propertyName">The name of the property. Automatically supplied by the compiler.</param>
    /// <param name="exceptionMessage">An optional custom exception message.</param>
    /// <returns>The original value if the check passes.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is negative.</exception>
    /// <remarks>
    ///     This method is intended for use in property setters and initializers.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NonNegative<T>(
        T value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
    {
        return Guard.NonNegative(value, propertyName, exceptionMessage);
    }
}