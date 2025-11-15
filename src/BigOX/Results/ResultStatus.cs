namespace BigOX.Results;

/// <summary>
///     Status values for a <see cref="IResult" />.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    ///     Default/zero value; no state established.
    /// </summary>
    Uninitialized = 0,

    /// <summary>
    ///     The result represents a failure.
    /// </summary>
    Failure = 1,

    /// <summary>
    ///     The result represents a success.
    /// </summary>
    Success = 2
}