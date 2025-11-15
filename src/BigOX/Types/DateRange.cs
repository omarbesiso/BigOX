using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;
using BigOX.Validation;

// ReSharper disable MemberCanBePrivate.Global

namespace BigOX.Types;

/// <summary>
///     Represents an <b>inclusive</b> date range using <see cref="DateOnly" /> with an optional open end.
///     The canonical, culture-invariant string form is <c>yyyy-MM-dd|yyyy-MM-dd</c> or <c>yyyy-MM-dd|∞</c>.
///     Parsers accept optional whitespace around tokens; formatters always emit the canonical form.
/// </summary>
/// <remarks>
///     <para>Default value: <c>default(DateRange)</c> equals <c>DateOnly.MinValue|∞</c>.</para>
///     <para>For calculations on open-ended ranges, see <see cref="EffectiveEnd" />.</para>
/// </remarks>
[JsonConverter(typeof(DateRangeConverter))]
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct DateRange : IEquatable<DateRange>, ISpanFormattable, ISpanParsable<DateRange>
{
    private const char RangeSeparator = '|';
    private const string DateFormat = "yyyy-MM-dd";
    private const char InfinityChar = '\u221E'; // "∞"

    private const int OpenEndedFormattedLengthConst = 10 /*yyyy-MM-dd*/ + 1 /*|*/ + 1 /*∞*/;
    internal static readonly int OpenEndedFormattedLength = OpenEndedFormattedLengthConst;
    internal static readonly int MaxFormattedLength = DateFormat.Length + 1 + DateFormat.Length; // closed range length

    /// <summary>
    ///     Gets the latest supported <see cref="DateOnly" /> value.
    ///     Used as the <em>effective</em> end for calculations on open-ended ranges;
    ///     note that <see cref="EndDate" /> remains <c>null</c> for open-ended ranges.
    /// </summary>
    internal static readonly DateOnly MaxSupportedDate = DateOnly.MaxValue;

    internal const string InvalidFormatMessage =
        "Invalid DateRange. Expected 'yyyy-MM-dd|yyyy-MM-dd' or 'yyyy-MM-dd|∞' (whitespace around tokens allowed).";

    /// <summary>
    ///     Initializes a new instance of <see cref="DateRange" />.
    /// </summary>
    /// <param name="startDate">The inclusive start of the range.</param>
    /// <param name="endDate">The inclusive end of the range; <c>null</c> denotes an open-ended range.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown if <paramref name="endDate" /> is earlier than <paramref name="startDate" />.
    /// </exception>
    public DateRange(DateOnly startDate, DateOnly? endDate = null)
    {
        ValidateDates(startDate, endDate, nameof(startDate));
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>The inclusive start date of the range.</summary>
    public DateOnly StartDate { get; }

    /// <summary>The inclusive end date of the range, or <c>null</c> for an open-ended range.</summary>
    public DateOnly? EndDate { get; }

    /// <summary>Indicates whether the range is open-ended (i.e., <see cref="EndDate" /> is <c>null</c>).</summary>
    /// <remarks>
    ///     When open-ended, <see cref="EffectiveEnd" /> returns <see cref="MaxSupportedDate" />, but
    ///     <see cref="EndDate" /> remains <c>null</c>.
    /// </remarks>
    public bool IsOpenEnded => EndDate is null;

    /// <summary>
    ///     Gets the inclusive end date to use for calculations.
    ///     Returns <see cref="EndDate" /> when present; otherwise <see cref="MaxSupportedDate" />.
    ///     This property does not change <see cref="EndDate" />.
    /// </summary>
    public DateOnly EffectiveEnd => EndDate ?? MaxSupportedDate;

    /// <inheritdoc />
    public bool Equals(DateRange other)
    {
        return StartDate.Equals(other.StartDate) && Nullable.Equals(EndDate, other.EndDate);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DateRange other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(StartDate, EndDate);
    }

    /// <summary>Compares two <see cref="DateRange" /> instances for equality.</summary>
    public static bool operator ==(DateRange left, DateRange right)
    {
        return left.Equals(right);
    }

    /// <summary>Compares two <see cref="DateRange" /> instances for inequality.</summary>
    public static bool operator !=(DateRange left, DateRange right)
    {
        return !left.Equals(right);
    }

    /// <summary>Factory method equivalent to the constructor.</summary>
    public static DateRange Create(DateOnly startDate, DateOnly? endDate = null)
    {
        return new DateRange(startDate, endDate);
    }

    /// <summary>
    ///     Returns the canonical string: <c>yyyy-MM-dd|yyyy-MM-dd</c> or <c>yyyy-MM-dd|∞</c>.
    ///     Uses invariant culture, ASCII digits, and Unicode infinity (<c>∞</c>) for open-ended ranges.
    /// </summary>
    public override string ToString()
    {
        Span<char> buffer = stackalloc char[MaxFormattedLength];
        if (TryFormat(buffer, out var written, default, CultureInfo.InvariantCulture))
        {
            return new string(buffer[..written]);
        }

        // Rare fallback path; manually build without interpolation overhead.
        Span<char> fb = stackalloc char[IsOpenEnded ? OpenEndedFormattedLength : MaxFormattedLength];
        if (TryFormat(fb, out written, default, CultureInfo.InvariantCulture))
        {
            return new string(fb[..written]);
        }

        // Should never happen; return empty string to avoid throwing inside ToString.
        return string.Empty;
    }

    /// <summary>
    ///     Formats using the canonical invariant representation. The <paramref name="format" /> is ignored.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return ToString();
    }

    /// <summary>
    ///     Attempts to format the canonical invariant representation into <paramref name="destination" />.
    ///     Only the general (canonical) format is supported; <paramref name="format" /> and <paramref name="provider" /> are
    ///     ignored.
    /// </summary>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        charsWritten = 0;
        var endIsOpen = !EndDate.HasValue;
        var needed = endIsOpen ? OpenEndedFormattedLength : MaxFormattedLength;
        if (destination.Length < needed)
        {
            return false;
        }

        if (!StartDate.TryFormat(destination, out var w1, DateFormat.AsSpan(), CultureInfo.InvariantCulture))
        {
            return false;
        }

        destination[w1++] = RangeSeparator;

        if (endIsOpen)
        {
            destination[w1++] = InfinityChar;
            charsWritten = w1;
            return true;
        }

        if (!EndDate!.Value.TryFormat(destination[w1..], out var w2, DateFormat.AsSpan(), CultureInfo.InvariantCulture))
        {
            return false;
        }

        charsWritten = w1 + w2;
        return true;
    }

    /// <summary>
    ///     Attempts to parse a <see cref="DateRange" /> from <paramref name="input" />.
    ///     Expected formats: <c>yyyy-MM-dd|yyyy-MM-dd</c> or <c>yyyy-MM-dd|∞</c>.
    ///     Parsing is invariant and requires exactly one <c>'|'</c> separator (whitespace around tokens is allowed).
    /// </summary>
    public static bool TryParse(string? input, out DateRange range)
    {
        if (input is null)
        {
            range = default;
            return false;
        }

        return TryParse(input.AsSpan(), out range);
    }

    /// <summary>
    ///     Span-based equivalent of <see cref="TryParse(string?, out DateRange)" />.
    /// </summary>
    public static bool TryParse(ReadOnlySpan<char> input, out DateRange range)
    {
        range = default;
        if (input.IsEmpty)
        {
            return false;
        }

        var s = input.Trim();
        // Single-pass separator discovery ensuring exactly one '|'.

        var sepIndex = -1;
        for (var i = 0; i < s.Length; i++)
        {
            if (s[i] == RangeSeparator)
            {
                if (sepIndex >= 0)
                {
                    return false; // second separator -> invalid
                }

                sepIndex = i;
            }
        }

        if (sepIndex <= 0 || sepIndex >= s.Length - 1)
        {
            return false;
        }

        var left = s[..sepIndex].Trim();
        if (!DateOnly.TryParseExact(left, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
        {
            return false;
        }

        var right = s[(sepIndex + 1)..].Trim();
        if (right.Length == 1 && right[0] == InfinityChar)
        {
            range = new DateRange(start);
            return true;
        }

        if (!DateOnly.TryParseExact(right, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
        {
            return false;
        }

        if (end < start)
        {
            return false;
        }

        range = new DateRange(start, end);
        return true;
    }

    /// <summary>Parses a <see cref="DateRange" /> or throws <see cref="FormatException" /> if invalid.</summary>
    public static DateRange Parse(string input)
    {
        if (TryParse(input, out var range))
        {
            return range;
        }

        throw new FormatException(InvalidFormatMessage);
    }

    /// <summary>Deconstructs the range into <paramref name="startDate" /> and <paramref name="endDate" />.</summary>
    public void Deconstruct(out DateOnly startDate, out DateOnly? endDate)
    {
        startDate = StartDate;
        endDate = EndDate;
    }

    /// <summary>Validates arguments for constructing a <see cref="DateRange" />.</summary>
    private static void ValidateDates(DateOnly startDateToValidate, DateOnly? endDateToValidate,
        string startDateParamName)
    {
        if (endDateToValidate.HasValue)
        {
            Guard.Maximum(startDateToValidate, endDateToValidate.Value, startDateParamName,
                "Start date cannot be after end date.");
        }
    }

    /// <inheritdoc />
    public static DateRange Parse(string s, IFormatProvider? provider)
    {
        return Parse(s);
    }

    /// <inheritdoc />
    public static bool TryParse(string? s, IFormatProvider? provider, out DateRange result)
    {
        return TryParse(s, out result);
    }

    /// <inheritdoc />
    public static DateRange Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (TryParse(s, out var result))
        {
            return result;
        }

        throw new FormatException(InvalidFormatMessage);
    }

    /// <inheritdoc />
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out DateRange result)
    {
        return TryParse(s, out result);
    }
}