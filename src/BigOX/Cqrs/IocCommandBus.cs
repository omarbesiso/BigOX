using BigOX.Results;
using BigOX.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Cqrs;

/// <summary>
///     Default implementation for the command bus using dependency injection for routing commands.
/// </summary>
internal class IocCommandBus(IServiceProvider serviceProvider) : ICommandBus
{
    /// <inheritdoc />
    public async Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        Guard.NotNull(command);
        var commandHandler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await commandHandler.Handle(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IResult<TValue>> Send<TCommand, TValue>(TCommand command,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        Guard.NotNull(command);
        var commandHandler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TValue>>();
        return await commandHandler.Handle(command, cancellationToken);
    }
}