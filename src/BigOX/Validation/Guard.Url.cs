using System.Runtime.CompilerServices;
using BigOX.Internals;

namespace BigOX.Validation;

/// <summary>
///     Provides guard-style argument validation helpers.
/// </summary>
public static partial class Guard
{
    /// <summary>
    ///     Ensures that a string is either <see langword="null" /> <strong>or</strong> a syntactically valid
    ///     absolute URL whose scheme is <c>http</c> or <c>https</c>.
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
    ///     <paramref name="value" /> when it is <see langword="null" /> or a valid HTTP/HTTPS URL.
    /// </returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="value" /> is non-<see langword="null" /> and not a valid HTTP/HTTPS URL.
    /// </exception>
    /// <remarks>
    ///     <para>
    ///         Validation uses <see cref="Uri.TryCreate(string,UriKind,out Uri?)" /> and checks the
    ///         <see cref="Uri.Scheme" /> against <c>http</c> / <c>https</c>.
    ///         If you need to allow additional schemes (e.g. <c>ftp</c>, <c>mailto</c>), wrap this helper
    ///         or copy it with a custom scheme set.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? callback = GetCallbackUrl();
    /// callback = Guard.Url(callback);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Url(
        string? value,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        // nulls are permitted; use NotNull/NotNullOrEmpty when null is unacceptable.
        if (value is null)
        {
            return value;
        }

        var ok = Uri.TryCreate(value, UriKind.Absolute, out var uri)
                 && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

        if (ok)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' is not a valid URL."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;
    }
}