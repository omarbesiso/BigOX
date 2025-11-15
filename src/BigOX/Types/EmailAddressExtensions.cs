using System.Net.Mail;
using System.Text;

namespace BigOX.Types;

/// <summary>
///     Presentation and interop helpers for the <see cref="EmailAddress" /> value type.
/// </summary>
/// <remarks>
///     <para>
///         These helpers operate only on already-normalized data exposed by <see cref="EmailAddress" /> (they never
///         re-parse).
///     </para>
///     <para>Complexity: per-call work is O(n) only for simple scans (username / domain extraction).</para>
///     <para>
///         <see cref="ToDisplayString" /> uses <see cref="MailAddress" /> only when a display name exists so that
///         quoting / escaping rules are applied correctly; otherwise the raw address is returned unchanged.
///     </para>
///     <para>All <c>ToMailAddress</c> overloads delegate to a single factory to centralize trimming and encoding handling.</para>
/// </remarks>
/// <seealso cref="EmailAddress" />
public static class EmailAddressExtensions
{
    /// <summary>
    ///     Internal factory for <see cref="MailAddress" /> creation used by the public overloads.
    /// </summary>
    /// <param name="address">Normalized email address string (may be empty).</param>
    /// <param name="displayName">Optional display name; trimmed when present.</param>
    /// <param name="encoding">Optional encoding to apply to the display name.</param>
    /// <returns>
    ///     A new <see cref="MailAddress" />. When <paramref name="address" /> is null / whitespace a minimal instance is
    ///     returned (will throw if constructed with invalid data under runtime rules).
    /// </returns>
    private static MailAddress CreateMailAddress(string address, string? displayName, Encoding? encoding)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            // Defensive: keep behavior consistent with prior implementation (MailAddress will throw for empty string).
            return new MailAddress(string.Empty);
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return new MailAddress(address);
        }

        var trimmed = displayName.Trim();
        return encoding is null
            ? new MailAddress(address, trimmed)
            : new MailAddress(address, trimmed, encoding);
    }

    /// <param name="emailAddress">Source <see cref="EmailAddress" /> value to extend.</param>
    extension(EmailAddress emailAddress)
    {
        /// <summary>
        ///     Formats a value for display. Returns the compact form (<c>address@example.com</c>) when no display name
        ///     exists; otherwise returns <c>Display Name &lt;address@example.com&gt;</c> with correct quoting/escaping.
        /// </summary>
        /// <remarks>
        ///     Empty / default values yield <see cref="string.Empty" /> to avoid misleading sentinel output.
        /// </remarks>
        /// <returns>User-friendly formatted string suitable for UI, logging and header construction.</returns>
        public string ToDisplayString()
        {
            // Guard against the default sentinel; return empty to avoid misleading output.
            if (emailAddress.IsEmpty || string.IsNullOrEmpty(emailAddress.Address))
            {
                return string.Empty;
            }

            var display = emailAddress.DisplayName;
            // Use MailAddress to correctly quote/escape display names when needed.
            return display is { Length: > 0 }
                ? new MailAddress(emailAddress.Address, display).ToString()
                : emailAddress.Address;
        }

        /// <summary>
        ///     Determines if a non-empty display name is present.
        /// </summary>
        /// <returns><c>true</c> when <see cref="EmailAddress.DisplayName" /> has length &gt; 0; otherwise <c>false</c>.</returns>
        public bool HasDisplayName()
        {
            return emailAddress.DisplayName is { Length: > 0 };
        }

        /// <summary>
        ///     Gets the local-part (username) portion preceding '@'. If '@' is missing (e.g. invalid/default value) the
        ///     entire address is returned.
        /// </summary>
        /// <returns>The username/local-part or <see cref="string.Empty" /> when the underlying address is empty.</returns>
        public string Username()
        {
            var address = emailAddress.Address;
            if (string.IsNullOrEmpty(address))
            {
                return string.Empty;
            }

            var at = address.IndexOf('@');
            return at >= 0 ? address[..at] : address;
        }

        /// <summary>
        ///     Gets the domain (host) portion following '@'. Returns <see cref="string.Empty" /> if '@' is absent or the
        ///     address is empty.
        /// </summary>
        /// <returns>The domain component or <see cref="string.Empty" />.</returns>
        public string Domain()
        {
            var address = emailAddress.Address;
            if (string.IsNullOrEmpty(address))
            {
                return string.Empty;
            }

            var at = address.IndexOf('@');
            return at >= 0 && at + 1 < address.Length ? address[(at + 1)..] : string.Empty;
        }

        /// <summary>
        ///     Alias for <see cref="Domain" /> for discoverability.
        /// </summary>
        /// <returns>The domain component or <see cref="string.Empty" />.</returns>
        public string Host()
        {
            return Domain(emailAddress);
        }

        /// <summary>
        ///     Creates a <see cref="MailAddress" /> preserving the stored display name (if any).
        /// </summary>
        /// <remarks>
        ///     Underlying <see cref="MailAddress" /> construction may throw <see cref="FormatException" /> if the address
        ///     is not in a recognized format (should not occur for normalized instances).
        /// </remarks>
        /// <returns>A new <see cref="MailAddress" /> reflecting the current value.</returns>
        public MailAddress ToMailAddress()
        {
            var display = emailAddress.DisplayName;
            return CreateMailAddress(emailAddress.Address, display, null);
        }

        /// <summary>
        ///     Creates a <see cref="MailAddress" /> overriding the display name. Null or whitespace omits a display name.
        /// </summary>
        /// <param name="displayName">Replacement display name; <c>null</c>/<c>whitespace</c> for none.</param>
        /// <returns>A new <see cref="MailAddress" /> instance.</returns>
        /// <exception cref="FormatException">Underlying address or combined parts are invalid.</exception>
        public MailAddress ToMailAddress(string? displayName)
        {
            return CreateMailAddress(emailAddress.Address, displayName, null);
        }

        /// <summary>
        ///     Creates a <see cref="MailAddress" /> overriding the display name and optionally its encoding.
        /// </summary>
        /// <param name="displayName">Replacement display name; <c>null</c>/<c>whitespace</c> for none.</param>
        /// <param name="displayNameEncoding">Encoding for the display name; <c>null</c> uses framework default.</param>
        /// <returns>A new <see cref="MailAddress" /> instance.</returns>
        /// <exception cref="FormatException">Underlying address or combined parts are invalid.</exception>
        public MailAddress ToMailAddress(string? displayName,
            Encoding? displayNameEncoding)
        {
            return CreateMailAddress(emailAddress.Address, displayName, displayNameEncoding);
        }
    }
}