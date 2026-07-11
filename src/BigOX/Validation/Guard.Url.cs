using System.Diagnostics.CodeAnalysis;
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
    [return: NotNullIfNotNull(nameof(value))]
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

    /// <summary>
    ///     Ensures that a string is either <see langword="null" /> <strong>or</strong> a syntactically valid absolute
    ///     URL whose scheme is one of <paramref name="allowedSchemes" /> (compared case-insensitively).
    /// </summary>
    /// <param name="value">The string to validate. May be <see langword="null" />.</param>
    /// <param name="allowedSchemes">
    ///     The set of permitted URI schemes (for example <c>http</c>, <c>https</c>, <c>ftp</c>). Each is compared to the
    ///     parsed <see cref="Uri.Scheme" /> using <see cref="StringComparison.OrdinalIgnoreCase" />. Must be
    ///     non-<see langword="null" /> and contain at least one element.
    /// </param>
    /// <param name="paramName">
    ///     The name of the argument being validated, automatically captured via
    ///     <see cref="CallerArgumentExpressionAttribute" /> when omitted.
    /// </param>
    /// <param name="exceptionMessage">
    ///     Optional custom message used when <paramref name="value" /> is non-null and invalid.
    ///     If omitted, a default message is generated.
    /// </param>
    /// <returns>
    ///     <paramref name="value" /> when it is <see langword="null" /> or a valid absolute URL using an allowed scheme.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="allowedSchemes" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="allowedSchemes" /> is empty, or when <paramref name="value" /> is
    ///     non-<see langword="null" /> and is not a valid absolute URL using one of the allowed schemes.
    /// </exception>
    /// <remarks>
    ///     <see cref="Uri.Scheme" /> is always lower-cased by <see cref="Uri" />, so schemes may be supplied in any case.
    /// </remarks>
    /// <example>
    ///     <code language="csharp"><![CDATA[
    /// string? endpoint = GetEndpoint();
    /// endpoint = Guard.Url(endpoint, ["https", "wss"]);
    /// ]]></code>
    /// </example>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? Url(
        string? value,
        string[] allowedSchemes,
        [CallerArgumentExpression(nameof(value))]
        string paramName = "",
        string? exceptionMessage = null)
    {
        ArgumentNullException.ThrowIfNull(allowedSchemes);

        if (allowedSchemes.Length == 0)
        {
            ThrowHelper.ThrowArgument(nameof(allowedSchemes), "At least one allowed scheme must be specified.");
        }

        // nulls are permitted; use NotNull/NotNullOrEmpty when null is unacceptable.
        if (value is null)
        {
            return value;
        }

        var ok = Uri.TryCreate(value, UriKind.Absolute, out var uri)
                 && SchemeIsAllowed(uri.Scheme, allowedSchemes);

        if (ok)
        {
            return value;
        }

        var message = string.IsNullOrWhiteSpace(exceptionMessage)
            ? $"The value of '{paramName}' is not a valid URL."
            : exceptionMessage;

        ThrowHelper.ThrowArgument(paramName, message);

        return value;

        static bool SchemeIsAllowed(string scheme, string[] schemes)
        {
            foreach (var candidate in schemes)
            {
                if (string.Equals(scheme, candidate, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}