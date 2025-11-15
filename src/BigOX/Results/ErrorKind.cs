namespace BigOX.Results;

/// <summary>
///     Lightweight, string-backed discriminator for categorizing errors (domain-specific taxonomy).
/// </summary>
/// <remarks>
///     Immutable value type; equality and hashing are based on <see cref="Value" />.
/// </remarks>
public readonly record struct ErrorKind
{
    /// <summary>
    ///     The default catch-all error kind.
    /// </summary>
    public static readonly ErrorKind Default = new("Default");

    /// <summary>
    ///     An error kind representing unexpected, unclassified failures.
    /// </summary>
    public static readonly ErrorKind Unexpected = new("Unexpected");

    /// <summary>
    ///     Initializes a new <see cref="ErrorKind" /> with the provided string value.
    /// </summary>
    /// <param name="value">Non-empty discriminator text.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is null, empty, or whitespace.</exception>
    public ErrorKind(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>
    ///     Gets the raw string value backing this kind.
    /// </summary>
    public string Value { get; }

    /// <summary>
    ///     Returns the raw <see cref="Value" />.
    /// </summary>
    public override string ToString()
    {
        return Value;
    }

    /// <summary>
    ///     Returns a hash code based on <see cref="Value" />.
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    ///     Creates a new <see cref="ErrorKind" /> from a string.
    /// </summary>
    /// <param name="value">Non-empty kind value.</param>
    /// <returns>A new <see cref="ErrorKind" />.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value" /> is null, empty, or whitespace.</exception>
    public static ErrorKind FromString(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new ErrorKind(value);
    }
}