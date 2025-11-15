namespace BigOX.Results;

/// <summary>
///     Convenience extensions over <see cref="IResult" />.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    ///     Provides extension methods for <see cref="IResult" />.
    /// </summary>
    extension(IResult result)
    {
        /// <summary>
        ///     Returns true when the result is a success.
        /// </summary>
        public bool IsSuccess => result.Status == ResultStatus.Success;

        /// <summary>
        ///     Returns true when the result is a failure.
        /// </summary>
        public bool IsFailure => result.Status == ResultStatus.Failure;

        /// <summary>
        ///     Returns all errors matching the specified <paramref name="code" />.
        /// </summary>
        /// <param name="code">Exact code to match (ordinal comparison).</param>
        /// <returns>An immutable snapshot of matching errors.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="code" /> is null, empty, or whitespace.</exception>
        public IReadOnlyCollection<IError> ErrorsByCode(string code)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(code);
            return result.Errors.Where(e => string.Equals(e.Code, code, StringComparison.Ordinal)).ToArray();
        }

        /// <summary>
        ///     Returns all errors matching the specified <paramref name="kind" />.
        /// </summary>
        /// <param name="kind">Kind to match.</param>
        /// <returns>An immutable snapshot of matching errors.</returns>
        public IReadOnlyCollection<IError> ErrorsByKind(ErrorKind kind)
        {
            return result.Errors.Where(e => e.Kind == kind).ToArray();
        }
    }
}