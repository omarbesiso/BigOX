using System.Diagnostics;
using System.Globalization;
using System.Net.Mail;
using BigOX.Validation;

// ReSharper disable RedundantExtendsListEntry

namespace BigOX.Types;

/// <summary>
///     Represents an immutable, normalized email address with an optional display name.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="EmailAddress" /> record struct normalizes the address portion to lowercase invariant
///         and converts the display name (when provided) to title case using the supplied <see cref="IFormatProvider" />
///         (or <see cref="CultureInfo.InvariantCulture" /> when none is specified).
///     </para>
///     <para>
///         Equality and hashing are performed in a case-insensitive manner on the <see cref="Address" /> only.
///         The display name does not participate in equality or hashing.
///     </para>
///     <para>Supported composite format strings in <see cref="ToString(string?, IFormatProvider?)" />:</para>
///     <list type="table">
///         <listheader>
///             <term>Format</term>
///             <description>Output</description>
///         </listheader>
///         <item>
///             <term>"A" / "a"</term>
///             <description>Address only (e.g. <c>user@example.com</c>).</description>
///         </item>
///         <item>
///             <term>"F" / "f" / "G" / "g"</term>
///             <description>
///                 Full form; address only if display name is null, otherwise <c>Display Name &lt;address&gt;</c>
///                 .
///             </description>
///         </item>
///     </list>
///     <para>
///         Use <see cref="From(string?, string?)" /> for explicit construction with validation; use
///         <see cref="Parse(string, IFormatProvider?)" />
///         and <see cref="TryParse(string?, IFormatProvider?, out EmailAddress)" /> for parsing arbitrary input strings.
///     </para>
///     <example>
///     </example>
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq")]
public readonly record struct EmailAddress :
    IComparable<EmailAddress>,
    IParsable<EmailAddress>,
    IEquatable<EmailAddress>,
    IFormattable
{
    private static readonly StringComparer AddressComparer = StringComparer.OrdinalIgnoreCase;
    private static readonly StringComparer DisplayComparer = StringComparer.Ordinal;

    private readonly string? _address;

    private EmailAddress(string address, string? displayName)
    {
        _address = address;
        DisplayName = displayName;
    }

    /// <summary>
    ///     Gets the normalized email address portion (always lowercase invariant). Returns <see cref="string.Empty" /> if
    ///     uninitialized.
    /// </summary>
    public string Address => _address ?? string.Empty;

    /// <summary>
    ///     Gets the (normalized) display name, or <c>null</c> if none was provided.
    /// </summary>
    /// <remarks>
    ///     Display names are normalized to title case using the culture specified during parsing or creation.
    /// </remarks>
    public string? DisplayName { get; }

    /// <summary>
    ///     Gets a value indicating whether this instance represents an empty (default) email address.
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Address);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#pragma warning disable IDE0051
    private string DebuggerDisplay => DisplayName is null ? Address : $"{DisplayName} <{Address}>";
#pragma warning restore IDE0051

    /// <summary>
    ///     Compares this instance with another <see cref="EmailAddress" /> for ordering.
    /// </summary>
    /// <param name="other">The other email address to compare to.</param>
    /// <returns>An integer that indicates the relative order: less than zero, zero, or greater than zero.</returns>
    /// <remarks>
    ///     Comparison first uses a case-insensitive comparison on <see cref="Address" />; if equal, a case-sensitive
    ///     comparison
    ///     on <see cref="DisplayName" /> is performed.
    /// </remarks>
    public int CompareTo(EmailAddress other)
    {
        var byAddress = AddressComparer.Compare(Address, other.Address);
        if (byAddress != 0)
        {
            return byAddress;
        }

        return DisplayComparer.Compare(DisplayName, other.DisplayName);
    }

    /// <summary>
    ///     Indicates whether the current object is equal to another <see cref="EmailAddress" /> instance.
    /// </summary>
    /// <param name="other">The other email address.</param>
    /// <returns><c>true</c> if the addresses are equal (case-insensitive); otherwise <c>false</c>.</returns>
    /// <remarks>
    ///     Only the <see cref="Address" /> is considered for equality. The <see cref="DisplayName" /> is ignored.
    /// </remarks>
    public bool Equals(EmailAddress other)
    {
        return AddressComparer.Equals(Address, other.Address);
    }

    /// <summary>
    ///     Formats the email address using a specified format string and format provider.
    /// </summary>
    /// <param name="format">A format string: "A"/"a" for address only; "F"/"f"/"G"/"g" for full form.</param>
    /// <param name="formatProvider">Ignored (reserved for future use). May be <c>null</c>.</param>
    /// <returns>The formatted email address string.</returns>
    /// <exception cref="FormatException">Thrown if an unsupported format string is provided.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        var fmt = string.IsNullOrEmpty(format) ? "G" : format;
        return fmt switch
        {
            "A" or "a" => Address,
            "F" or "f" or "G" or "g" => DisplayName is null ? Address : $"{DisplayName} <{Address}>",
            _ => throw new FormatException(
                $"The {nameof(EmailAddress)} format string '{format}' is not supported. Use 'G', 'A', or 'F'.")
        };
    }

    /// <summary>
    ///     Parses a string into an <see cref="EmailAddress" /> instance.
    /// </summary>
    /// <param name="s">A string containing an email address, optionally with a display name.</param>
    /// <param name="provider">An optional <see cref="IFormatProvider" /> influencing display name normalization.</param>
    /// <returns>A new <see cref="EmailAddress" /> instance.</returns>
    /// <exception cref="FormatException">
    ///     Thrown if <paramref name="s" /> is null, whitespace, or not a recognized email
    ///     address format.
    /// </exception>
    public static EmailAddress Parse(string s, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            throw new FormatException("Input cannot be null or whitespace.");
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (!MailAddress.TryCreate(s.Trim(), out var parsed) || parsed is null)
        {
            throw new FormatException("The input string is not a recognized email address.");
        }

        return CreateFromParsed(parsed, provider);
    }

    /// <summary>
    ///     Attempts to parse a string into an <see cref="EmailAddress" /> instance.
    /// </summary>
    /// <param name="s">A string containing an email address, optionally with a display name.</param>
    /// <param name="provider">An optional <see cref="IFormatProvider" /> influencing display name normalization.</param>
    /// <param name="result">
    ///     When this method returns, contains the parsed email address if successful; otherwise the default
    ///     value.
    /// </param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    public static bool TryParse(string? s, IFormatProvider? provider, out EmailAddress result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (!MailAddress.TryCreate(s.Trim(), out var parsed) || parsed is null)
        {
            return false;
        }

        result = CreateFromParsed(parsed, provider);
        return true;
    }

    /// <summary>
    ///     Creates an <see cref="EmailAddress" /> from separate email and display name components.
    /// </summary>
    /// <param name="email">The raw email address. Must not be null or whitespace.</param>
    /// <param name="displayName">An optional raw display name.</param>
    /// <returns>A new validated and normalized <see cref="EmailAddress" /> instance.</returns>
    /// <exception cref="FormatException">
    ///     Thrown if the email or combined display name and email are not in a recognized
    ///     format.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="email" /> is null or whitespace.</exception>
    public static EmailAddress From(string? email, string? displayName = null)
    {
        Guard.NotNullOrWhiteSpace(email);
        var trimmedEmail = email.Trim();
        var trimmedDisplay = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        var success = trimmedDisplay is null
            ? MailAddress.TryCreate(trimmedEmail, out var parsed)
            : MailAddress.TryCreate(trimmedEmail, trimmedDisplay, out parsed);

        if (!success || parsed is null)
        {
            throw new FormatException(trimmedDisplay is null
                ? "The email address is not in a recognized format."
                : "The email address or display name is not in a recognized format.");
        }

        return CreateFromParsed(parsed, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Returns the full formatted representation using the invariant culture.
    /// </summary>
    /// <returns>A string representation of the email address.</returns>
    public override string ToString()
    {
        return ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     Constructs an <see cref="EmailAddress" /> from a successfully parsed <see cref="MailAddress" />.
    /// </summary>
    /// <param name="parsed">The parsed mail address.</param>
    /// <param name="provider">The culture provider used for display name normalization.</param>
    /// <returns>A normalized <see cref="EmailAddress" />.</returns>
    private static EmailAddress CreateFromParsed(MailAddress parsed, IFormatProvider? provider)
    {
        var normalizedAddress = NormalizeAddress(parsed.Address);
        var normalizedDisplay = NormalizeDisplayName(parsed.DisplayName, provider);
        return new EmailAddress(normalizedAddress, normalizedDisplay);
    }

    /// <summary>
    ///     Normalizes an email address string to lowercase invariant and trims surrounding whitespace.
    /// </summary>
    /// <param name="address">The address to normalize.</param>
    /// <returns>The normalized address.</returns>
    private static string NormalizeAddress(string address)
    {
        return address.Trim().ToLowerInvariant();
    }

    /// <summary>
    ///     Normalizes a display name to title case using the supplied provider (or invariant culture).
    /// </summary>
    /// <param name="displayName">The raw display name.</param>
    /// <param name="provider">The format provider determining cultural casing rules.</param>
    /// <returns>The normalized display name or <c>null</c> if input is null/whitespace.</returns>
    private static string? NormalizeDisplayName(string? displayName, IFormatProvider? provider)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return null;
        }

        var culture = provider as CultureInfo ?? CultureInfo.InvariantCulture;
        var lowered = displayName.Trim().ToLower(culture);
        return culture.TextInfo.ToTitleCase(lowered);
    }

    /// <summary>
    ///     Gets the hash code for this instance based on the normalized <see cref="Address" /> (case-insensitive).
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return AddressComparer.GetHashCode(Address);
    }
}