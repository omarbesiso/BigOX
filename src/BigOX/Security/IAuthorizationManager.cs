using System.Security;

namespace BigOX.Security;

/// <summary>
///     Defines the contract for orchestrating authorization rule evaluation.
/// </summary>
public interface IAuthorizationManager
{
    /// <summary>
    ///     Asynchronously evaluates all registered authorization rules for the specified
    ///     authorization arguments and returns an aggregated result.
    /// </summary>
    /// <typeparam name="TAuthorizationArgs">
    ///     The type of authorization arguments for which rules will be evaluated.
    /// </typeparam>
    /// <param name="authorizationArgs">
    ///     The authorization arguments to evaluate.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask{TResult}" /> whose result describes the outcome of rule evaluation.
    /// </returns>
    ValueTask<AuthorizationEvaluationResult> EvaluateAsync<TAuthorizationArgs>(
        TAuthorizationArgs authorizationArgs,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Asynchronously authorizes the specified arguments and throws a
    ///     <see cref="SecurityException" /> if authorization fails.
    /// </summary>
    /// <typeparam name="TAuthorizationArgs">
    ///     The type of authorization arguments for which rules will be evaluated.
    /// </typeparam>
    /// <param name="authorizationArgs">
    ///     The authorization arguments to evaluate.
    /// </param>
    /// <param name="cancellationToken">
    ///     The token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask" /> that completes when authorization succeeds
    ///     or throws if it fails.
    /// </returns>
    /// <exception cref="SecurityException">
    ///     Thrown when one or more authorization rules fail.
    /// </exception>
    ValueTask AuthorizeAsync<TAuthorizationArgs>(
        TAuthorizationArgs authorizationArgs,
        CancellationToken cancellationToken = default);
}