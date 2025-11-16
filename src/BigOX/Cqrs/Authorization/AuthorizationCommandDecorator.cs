using BigOX.Validation;
using BigOX.Security;

namespace BigOX.Cqrs.Authorization;

/// <summary>
///     Decorator for <see cref="ICommandHandler{TCommand}" /> that performs authorization
///     prior to invoking the inner handler.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <remarks>
///     Authorization is performed against the command instance itself. Register <see cref="IAuthorizationRule{TAuthorizationArgs}" />
///     implementations for the specific command type to participate in evaluation.
/// </remarks>
internal sealed class AuthorizationCommandDecorator<TCommand> : ICommandDecorator<TCommand>
    where TCommand : ICommand
{
    private readonly ICommandHandler<TCommand> _decorated;
    private readonly IAuthorizationManager _authorizationManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationCommandDecorator{TCommand}" /> class.
    /// </summary>
    /// <param name="decorated">The inner command handler.</param>
    /// <param name="authorizationManager">The authorization manager orchestrating rule evaluation.</param>
    public AuthorizationCommandDecorator(
        ICommandHandler<TCommand> decorated,
        IAuthorizationManager authorizationManager)
    {
        _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
        _authorizationManager = authorizationManager ?? throw new ArgumentNullException(nameof(authorizationManager));
    }

    /// <summary>
    ///     Authorizes and then handles the command.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
    {
        Guard.NotNull(command);

        // Authorization against the command instance itself.
        await _authorizationManager.AuthorizeAsync(command, cancellationToken).ConfigureAwait(false);

        await _decorated.Handle(command, cancellationToken).ConfigureAwait(false);
    }
}
