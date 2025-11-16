using BigOX.Results;

namespace BigOX.Cqrs;

/// <summary>
///     The contract defines the router of commands to command handlers.
/// </summary>
public interface ICommandBus
{
    /// <summary>
    ///     Routes the specified command to the relevant command handler.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="command">The command to be routed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;

    /// <summary>
    ///     Routes the specified command to the relevant command handler and returns a value result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TValue">The type of the value produced on successful command handling.</typeparam>
    /// <param name="command">The command to be routed.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="IResult{TValue}" /> describing the outcome of the command.
    /// </returns>
    Task<IResult<TValue>> Send<TCommand, TValue>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand;
}