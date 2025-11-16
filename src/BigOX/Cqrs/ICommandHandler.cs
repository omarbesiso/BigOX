using BigOX.Results;

namespace BigOX.Cqrs;

/// <summary>
///     Defines a contract for handling command as specified in the CQRS pattern.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, which must implement <see cref="ICommand" />.</typeparam>
/// <remarks>
///     The <see cref="ICommandHandler{TCommand}" /> interface defines a contract for handling a command of type
///     <typeparamref name="TCommand" />.
///     Classes that implement this interface are responsible for defining the logic for handling the command and must
///     provide an implementation for the <see cref="Handle" /> method.
///     <para>
///         Please note:
///     </para>
///     1. A command is a DTO representing a request for a change in the domain.
///     2. A single command usually is mapped to a single command handler.
///     3. In CQRS command handlers represent an implementation for Application Services (as defined in domain driven
///     design) that cause a change in the domain.
/// </remarks>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    ///     Handles the specified command.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task Handle(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
///     Defines a contract for handling a command and producing a value result.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle, which must implement <see cref="ICommand" />.</typeparam>
/// <typeparam name="TValue">The type of the value produced on successful command handling.</typeparam>
/// <remarks>
///     The <see cref="ICommandHandler{TCommand, TValue}" /> interface extends the CQRS command handling pattern by
///     allowing handlers to return a value wrapped in an <see cref="IResult{TValue}" />.
///     <para>
///         Use this variant when the caller requires a value (for example, an identifier or summary) in addition to the
///         success or failure information.
///     </para>
/// </remarks>
public interface ICommandHandler<in TCommand, TValue>
    where TCommand : ICommand
{
    /// <summary>
    ///     Handles the specified command and returns a result containing a value on success.
    /// </summary>
    /// <param name="command">The command to be handled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="IResult{TValue}" /> describing the outcome of the command.
    /// </returns>
    Task<IResult<TValue>> Handle(TCommand command, CancellationToken cancellationToken = default);
}