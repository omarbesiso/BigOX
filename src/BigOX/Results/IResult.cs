namespace BigOX.Results;

/// <summary>
///     Result with a value payload.
/// </summary>
/// <typeparam name="TValue">Type of the success value.</typeparam>
public interface IResult<out TValue> : IResult
{
    /// <summary>
    ///     Gets the success value when the result is in a success state; null when in a failure state.
    /// </summary>
    TValue? Value { get; }
}

/// <summary>
///     Common, non-generic result surface.
/// </summary>
public interface IResult
{
    /// <summary>
    ///     Optional human-readable message associated with the result.
    /// </summary>
    string? Message { get; }

    /// <summary>
    ///     Gets the status of the result.
    /// </summary>
    ResultStatus Status { get; }

    /// <summary>
    ///     Convenience boolean indicating a success state.
    /// </summary>
    bool IsSuccess => Status == ResultStatus.Success;

    /// <summary>
    ///     Convenience boolean indicating a failure state.
    /// </summary>
    bool IsFailure => Status == ResultStatus.Failure;

    /// <summary>
    ///     Gets the collection of errors (empty when not in a failure state).
    /// </summary>
    IReadOnlyList<IError> Errors { get; }

    /// <summary>
    ///     Optional, immutable metadata bag spanning the result (success or failure).
    /// </summary>
    IReadOnlyDictionary<string, object?> Metadata { get; }
}

/// <summary>
///     Result with a value and strongly-typed error items.
/// </summary>
/// <typeparam name="TValue">Type of the success value.</typeparam>
/// <typeparam name="TError">Type of the error items (must implement <see cref="IError" />).</typeparam>
public interface IResult<out TValue, out TError> : IResult<TValue>
    where TError : IError
{
    /// <summary>
    ///     Strongly-typed error collection (covariant).
    /// </summary>
    new IReadOnlyList<TError> Errors { get; }
}