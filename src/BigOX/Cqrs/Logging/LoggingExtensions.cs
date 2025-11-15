using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Cqrs.Logging;

/// <summary>
///     Provides extension methods for decorating CQRS command and query handlers with logging capabilities.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    ///     Provides extension methods for decorating CQRS command and query handlers with logging capabilities.
    /// </summary>
    /// <param name="serviceCollection">The service collection to contain the registration.</param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Decorates the command handler with logging.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command.</typeparam>
        /// <returns>A reference to this service collection instance after the operation has completed.</returns>
        public IServiceCollection DecorateCommandHandlerWithLogging<TCommand>()
            where TCommand : ICommand
        {
            return serviceCollection.DecorateCommandHandler<TCommand, LoggingCommandDecorator<TCommand>>();
        }

        /// <summary>
        ///     Decorates the query handler with logging.
        /// </summary>
        /// <typeparam name="TQuery">The type of the query.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <returns>A reference to this service collection instance after the operation has completed.</returns>
        public IServiceCollection DecorateQueryHandlerWithLogging<TQuery, TResult>()
            where TQuery : IQuery
        {
            return serviceCollection.DecorateQueryHandler<TQuery, TResult, LoggingQueryDecorator<TQuery, TResult>>();
        }
    }
}