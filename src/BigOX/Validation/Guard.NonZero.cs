using System.Numerics;
using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a numeric value is <strong>not equal to zero</strong>.
    ///     Supports any unmanaged or managed numeric type that implements
    ///     <see cref="INumber{TSelf}" />—<c>int</c>, <c>long</c>, <c>decimal</c>, <c>double</c>,
    ///     <see cref="System.Numerics.BigInteger" />, custom numeric structs, etc.
    /// </summary>
    /// <typeparam name="T">
    ///     A numeric type that fulfils <see cref="INumber{TSelf}" />.
    /// </typeparam>
    /// <param name="value">The numeric value to validate.</param>
    /// <param name="paramName">
    ///     Argument name, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> equals its additive identity (<c>0</c>).
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     The original <paramref name="value" /> when it is not zero.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals its type’s additive identity.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         For floating-point types the comparison is exact—i.e., <c>0.0</c>.
    ///         If you need an epsilon-based comparison use a domain-specific helper.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// int denominator = 0;              // BUG
    /// denominator = Guard.NonZero(denominator); // throws
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NonZero<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
    {
        if (value != T.Zero)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must not be zero."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}