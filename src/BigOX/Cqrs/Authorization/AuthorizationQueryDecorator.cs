using BigOX.Security;
using BigOX.Validation;

namespace BigOX.Cqrs.Authorization;

/// <summary>
///     Decorator for <see cref="IQueryHandler{TQuery,TResult}" /> that performs authorization
///     prior to invoking the inner handler.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
/// <remarks>
///     Authorization is performed against the query instance itself. Register
///     <see cref="IAuthorizationRule{TAuthorizationArgs}" />
///     implementations for the specific query type to participate in evaluation.
/// </remarks>
internal sealed class AuthorizationQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    where TQuery : IQuery
{
    private readonly IAuthorizationManager _authorizationManager;
    private readonly IQueryHandler<TQuery, TResult> _decorated;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationQueryDecorator{TQuery,TResult}" /> class.
    /// </summary>
    /// <param name="decorated">The inner query handler.</param>
    /// <param name="authorizationManager">The authorization manager orchestrating rule evaluation.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="decorated" /> or <paramref name="authorizationManager" /> is
    ///     <see langword="null" />.
    /// </exception>
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
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="System.Security.SecurityException">
    ///     Thrown when one or more authorization rules fail.
    /// </exception>
    public async Task<TResult> Read(TQuery query, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(query);

        // Authorization against the query instance itself.
        await _authorizationManager.AuthorizeAsync(query, cancellationToken).ConfigureAwait(false);

        return await _decorated.Read(query, cancellationToken).ConfigureAwait(false);
    }
}