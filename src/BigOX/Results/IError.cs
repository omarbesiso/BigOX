using System.Text.Json.Serialization;

namespace BigOX.Results;

/// <summary>
///     Contract for an error item carried by a failed result.
/// </summary>
[JsonDerivedType(typeof(Error), "error")]
public interface IError
{
    /// <summary>
    ///     Human-readable, localized message describing the error.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    ///     Machine-oriented code identifying the error (defaults to <see cref="Kind" />.<see cref="ErrorKind.Value" />).
    /// </summary>
    string Code { get; }

    /// <summary>
    ///     Optional underlying exception (not serialized).
    /// </summary>
    [JsonIgnore]
    Exception? Exception { get; }

    /// <summary>
    ///     Domain-specific classification of the error.
    /// </summary>
    ErrorKind Kind { get; }

    /// <summary>
    ///     Optional, immutable metadata bag for diagnostics or domain context.
    /// </summary>
    IReadOnlyDictionary<string, object?> Metadata { get; }
}