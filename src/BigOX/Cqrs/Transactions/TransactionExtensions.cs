using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Cqrs.Transactions;

/// <summary>
///     Provides extension methods for decorating command handlers with transaction support.
/// </summary>
public static class TransactionExtensions
{
    /// <summary>
    ///     Decorates the command handler for the specified command type with a transaction decorator.
    ///     If no handler is registered for the command type, this is a no-op.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <param name="serviceCollection">The service collection to add the decorated command handler to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection DecorateCommandHandlerWithTransactions<TCommand>(
        this IServiceCollection serviceCollection)
        where TCommand : ICommand
    {
        // Avoid Scrutor.DecorationException by decorating only when a handler is already registered
        var serviceType = typeof(ICommandHandler<TCommand>);
        return serviceCollection.All(sd => sd.ServiceType != serviceType)
            ? serviceCollection
            : serviceCollection.DecorateCommandHandler<TCommand, DefaultTransactionCommandDecorator<TCommand>>();
    }
}