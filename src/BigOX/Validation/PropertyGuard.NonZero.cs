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
    ///     Ensures that a numeric property value is <strong>not equal to zero</strong>.
    /// </summary>
    /// <typeparam name="T">A numeric type that implements <see cref="INumber{TSelf}" />.</typeparam>
    /// <param name="value">The property value to validate.</param>
    /// <param name="propertyName">
    ///     Property name, automatically captured via
    ///     <see cref="CallerMemberNameAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> equals zero.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it is not zero.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals zero.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// private decimal _unitPrice = 0m;
    /// 
    /// public decimal UnitPrice
    /// {
    ///     get => _unitPrice;
    ///     set => _unitPrice = PropertyGuard.NonZero(value);
    /// }
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NonZero<T>(
        T value,
        [CallerMemberName] string propertyName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
    {
        return Guard.NonZero(value, propertyName, exceptionMessage);
    }
}