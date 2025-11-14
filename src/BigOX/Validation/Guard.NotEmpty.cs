using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that the specified <paramref name="value" /> is not <see cref="Guid.Empty" />.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when the GUID is empty.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>The original, non-empty <paramref name="value" />.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> equals <see cref="Guid.Empty" />.
    /// </exception>
    /// <remarks>
    ///     This helper is useful when validating identifiers to guarantee that an un-initialised or default
    ///     GUID is never accepted by business logic.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// Guid orderId = GetOrderId();
    /// orderId = Guard.NotEmpty(orderId); // throws if orderId == Guid.Empty
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid NotEmpty(
        Guid value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        if (value != Guid.Empty)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The GUID '{paramName}' cannot be empty."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}