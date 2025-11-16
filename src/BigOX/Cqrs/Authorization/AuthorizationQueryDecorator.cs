using BigOX.Validation;
using BigOX.Security;

namespace BigOX.Cqrs.Authorization;

/// <summary>
///     Decorator for <see cref="IQueryHandler{TQuery,TResult}" /> that performs authorization
///     prior to invoking the inner handler.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
/// <remarks>
///     Authorization is performed against the query instance itself. Register <see cref="IAuthorizationRule{TAuthorizationArgs}" />
///     implementations for the specific query type to participate in evaluation.
/// </remarks>
internal sealed class AuthorizationQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery
{
    private readonly IQueryHandler<TQuery, TResult> _decorated;
    private readonly IAuthorizationManager _authorizationManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationQueryDecorator{TQuery,TResult}" /> class.
    /// </summary>
    /// <param name="decorated">The inner query handler.</param>
    /// <param name="authorizationManager">The authorization manager orchestrating rule evaluation.</param>
    public AuthorizationQueryDecorator(
        IQueryHandler<TQuery, TResult> decorated,
        IAuthorizationManager authorizationManager)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _authorizationManager = authorizationManager ?? throw new ArgumentNullException(nameof(authorizationManager));
    }

    /// <summary>
    ///     Authorizes and then reads the query.
    /// </summary>
    /// <param name="query">The query instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query result produced by the inner handler.</returns>
    public async Task<TResult> Read(TQuery query, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(query);

        // Authorization against the query instance itself.
        await _authorizationManager.AuthorizeAsync(query, cancellationToken).ConfigureAwait(false);

        return await _decorated.Read(query, cancellationToken).ConfigureAwait(false);
    }
}
