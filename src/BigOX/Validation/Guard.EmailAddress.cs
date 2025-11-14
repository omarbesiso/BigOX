using System.Net.Mail;
using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a string is either <see langword="null" /> **or** a syntactically valid e-mail address.
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is non-null and invalid.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or a valid e-mail address.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and not a valid e-mail address.
    /// </exception>
    /// <remarks>
    ///     Uses <see cref="MailAddress.TryCreate(string?,out System.Net.Mail.MailAddress?)" /> to parse the address,
    ///     which handles quoted local parts and internationalised domain names.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? email = GetEmail();
    /// email = Guard.EmailAddress(email);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? EmailAddress(
        string? value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // A null value is allowed; use NotNull* if null should be rejected.
        if (value is null)
        {
            return value;
        }

        // Validate address (allow Unicode by default).
        if (MailAddress.TryCreate(value, out _))
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' is not a valid email address."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}