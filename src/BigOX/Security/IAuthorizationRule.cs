namespace BigOX.Security;

/// <summary>
///     Defines the contract for an authorization rule that evaluates whether
///     a given operation, represented by its authorization arguments, is allowed to proceed.
/// </summary>
/// <typeparam name="TAuthorizationArgs">
///     The type of arguments required for the authorization rule to execute.
/// </typeparam>
public interface IAuthorizationRule<in TAuthorizationArgs>
{
    /// <summary>
    ///     Asynchronously evaluates whether execution of a specific action is authorized.
    /// </summary>
    /// <param name="authorizationArgs">
    ///     The arguments required to execute this rule.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> whose result indicates whether the authorization
    ///     rule has passed or failed.
    /// </returns>
    /// <remarks>
    ///     Implementations should return <c>Successful = false</c> to indicate expected
    ///     authorization denials and reserve throwing exceptions for truly exceptional or
    ///     infrastructure-level failures (for example, configuration, connectivity, or I/O issues).
    /// </remarks>
    ValueTask<AuthorizationResult> IsAuthorizedAsync(
        TAuthorizationArgs authorizationArgs,
        CancellationToken cancellationToken = default);
}