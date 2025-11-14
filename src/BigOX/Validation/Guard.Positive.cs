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
    ///     Ensures that a numeric value is <strong>strictly greater than zero</strong>.
    /// </summary>
    /// <typeparam name="T">Any numeric type that implements <see cref="INumber{TSelf}" />.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">
    ///     Name of the argument being validated, captured automatically via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is not positive.
    /// </param>
    /// <returns>The original <paramref name="value" /> when it is &gt; 0.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> ≤ 0.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// int qty = Guard.Positive(order.Quantity);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Positive<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
    {
        if (value > T.Zero)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must be positive."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }

    /// <summary>
    ///     Ensures that a numeric value is <strong>zero or greater</strong>.
    /// </summary>
    /// <typeparam name="T">Any numeric type that implements <see cref="INumber{TSelf}" />.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">Argument name, captured automatically.</param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is negative.
    /// </param>
    /// <returns>The original <paramref name="value" /> when it is ≥ 0.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> &lt; 0.
    /// </exception>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// decimal balance = Guard.NonNegative(account.Balance);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T NonNegative<T>(
        T value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
        where T : INumber<T>
    {
        if (value >= T.Zero)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' must not be negative."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}