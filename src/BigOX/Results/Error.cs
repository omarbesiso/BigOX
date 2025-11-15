using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using BigOX.Extensions;

namespace BigOX.Results;

/// <summary>
///     Default concrete <see cref="IError" /> implementation.
/// </summary>
/// <remarks>
///     Instances are immutable. <see cref="Code" /> defaults to <see cref="Kind" />.<see cref="ErrorKind.Value" /> when
///     not supplied.
/// </remarks>
[JsonDerivedType(typeof(Error), "error")]
public sealed record Error : IError
{
    private Error(string errorMessage, string? code, ErrorKind? kind = null, Exception? exception = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        ErrorMessage = errorMessage;
        Kind = kind ?? ErrorKind.Default;
        Code = string.IsNullOrWhiteSpace(code) ? Kind.Value : code;
        Exception = exception;
        Metadata = metadata.FreezeOrEmpty();
    }

    /// <summary>
    ///     Gets the human-readable error message.
    /// </summary>
    public string ErrorMessage { get; init; }

    /// <summary>
    ///     Gets the machine-oriented error code.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    ///     Gets the optional underlying exception.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    ///     Gets the error kind.
    /// </summary>
    public ErrorKind Kind { get; init; }

    /// <summary>
    ///     Gets the immutable metadata dictionary.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Metadata { get; init; }

    /// <summary>
    ///     Creates a new <see cref="Error" />.
    /// </summary>
    /// <param name="message">Required message.</param>
    /// <param name="code">Optional code; defaults to <see cref="Kind" /> if omitted.</param>
    /// <param name="kind">Optional kind; defaults to <see cref="ErrorKind.Default" />.</param>
    /// <param name="exception">Optional exception.</param>
    /// <param name="metadata">Optional metadata; frozen for immutability.</param>
    /// <returns>A new <see cref="Error" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message" /> is null.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Create(
        string message,
        string? code = null,
        ErrorKind? kind = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Error(message ?? throw new ArgumentNullException(nameof(message)), code, kind, exception, metadata);
    }

    /// <summary>
    ///     Creates a new <see cref="Error" /> with <see cref="ErrorKind.Unexpected" />.
    /// </summary>
    /// <param name="message">Required message.</param>
    /// <param name="code">Optional code; defaults to <see cref="ErrorKind.Unexpected" />.<see cref="ErrorKind.Value" />.</param>
    /// <param name="exception">Optional exception.</param>
    /// <param name="metadata">Optional metadata; frozen for immutability.</param>
    /// <returns>A new unexpected <see cref="Error" />.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Error Unexpected(
        string message,
        string? code = null,
        Exception? exception = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new Error(message ?? throw new ArgumentNullException(nameof(message)), code, ErrorKind.Unexpected,
            exception,
            metadata);
    }
}