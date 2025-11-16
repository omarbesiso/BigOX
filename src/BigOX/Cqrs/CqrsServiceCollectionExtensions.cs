using BigOX.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable RedundantSwitchExpressionArms

namespace BigOX.Cqrs;

/// <summary>
///     Class providing extensions to the <see cref="IServiceCollection" /> to allow for the registration of different
///     types of handlers for different message types.
/// </summary>
public static class CqrsServiceCollectionExtensions
{
    /// <summary>
    ///     Decorates all registered command handlers with the specified decorator type.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to which the decorator should be applied.</param>
    /// <param name="commandDecoratorType">
    ///     The type of the decorator that implements <see cref="ICommandDecorator{TCommand}" />.
    /// </param>
    /// <returns>The service collection with all command handlers decorated.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="commandDecoratorType" /> does not implement
    ///     <see cref="ICommandDecorator{TCommand}" />.
    /// </exception>
    /// <remarks>
    ///     This method decorates all currently registered command handlers of type
    ///     <see cref="ICommandHandler{TCommand}" /> with the specified <paramref name="commandDecoratorType" />.
    ///     The decorator type must implement <see cref="ICommandDecorator{TCommand}" />, otherwise
    ///     an <see cref="ArgumentException" /> is thrown.
    /// </remarks>
    public static IServiceCollection DecorateAllCommandHandlers(IServiceCollection serviceCollection,
        Type commandDecoratorType)
    {
        // Allow open generic types that implement ICommandDecorator<>

        if (!ImplementsOpenGenericInterface(commandDecoratorType, typeof(ICommandDecorator<>)))
        {
            throw new ArgumentException("Decorator must implement ICommandDecorator<TCommand>.",
                commandDecoratorType.FullName);
        }

        serviceCollection.Decorate(typeof(ICommandHandler<>), commandDecoratorType);
        return serviceCollection;
    }

    /// <summary>
    ///     Decorates all registered query handlers with the specified decorator type.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to which the decorator should be applied.</param>
    /// <param name="queryDecoratorType">
    ///     The type of the decorator that implements <see cref="IQueryDecorator{TQuery, TResult}" />.
    /// </param>
    /// <returns>The service collection with all query handlers decorated.</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="queryDecoratorType" /> does not implement
    ///     <see cref="IQueryDecorator{TQuery, TResult}" />.
    /// </exception>
    /// <remarks>
    ///     This method decorates all currently registered query handlers of type
    ///     <see cref="IQueryHandler{TQuery, TResult}" /> with the specified <paramref name="queryDecoratorType" />.
    ///     The decorator type must implement <see cref="IQueryDecorator{TQuery, TResult}" />, otherwise
    ///     an <see cref="ArgumentException" /> is thrown.
    /// </remarks>
    public static IServiceCollection DecorateAllQueryHandlers(IServiceCollection serviceCollection,
        Type queryDecoratorType)
    {
        // Allow open generic types that implement IQueryDecorator<,>
        if (!ImplementsOpenGenericInterface(queryDecoratorType, typeof(IQueryDecorator<,>)))
        {
            throw new ArgumentException("Decorator must implement IQueryDecorator<TQuery, TResult>.",
                queryDecoratorType.FullName);
        }

        serviceCollection.Decorate(typeof(IQueryHandler<,>), queryDecoratorType);
        return serviceCollection;
    }

    private static bool ImplementsOpenGenericInterface(Type type, Type openGenericInterface)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (openGenericInterface is null)
        {
            throw new ArgumentNullException(nameof(openGenericInterface));
        }

        // If the type itself is an interface, check its generic definition
        if (type is { IsInterface: true, IsGenericType: true } && type.GetGenericTypeDefinition() == openGenericInterface)
        {
            return true;
        }

        // Check all implemented interfaces
        foreach (var iface in type.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == openGenericInterface)
            {
                return true;
            }
        }

        return false;
    }

    private static void RegisterInternal<TModule>(IServiceCollection serviceCollection, ServiceLifetime serviceLifetime,
        Type type, IEnumerable<Type>? excludedTypes = null)
        where TModule : IModule
    {
        var excludedTypesList = excludedTypes != null ? excludedTypes.ToList() : [];

        switch (serviceLifetime)
        {
            case ServiceLifetime.Scoped:
                serviceCollection.Scan(scan =>
                    scan.FromAssemblyOf<TModule>()
                        .AddClasses(classes =>
                            classes.AssignableTo(type)
                                .Where(t => !excludedTypesList.Any(t.IsBasedOn) && t.IsBasedOn(type)), false)
                        .AsImplementedInterfaces()
                        .WithScopedLifetime());
                break;
            case ServiceLifetime.Transient:
                serviceCollection.Scan(scan =>
                    scan.FromAssemblyOf<TModule>()
                        .AddClasses(classes =>
                            classes.AssignableTo(type)
                                .Where(t => !excludedTypesList.Any(t.IsBasedOn) && t.IsBasedOn(type)), false)
                        .AsImplementedInterfaces()
                        .WithTransientLifetime());
                break;
            case ServiceLifetime.Singleton:
                serviceCollection.Scan(scan =>
                    scan.FromAssemblyOf<TModule>()
                        .AddClasses(classes =>
                            classes.AssignableTo(type)
                                .Where(t => !excludedTypesList.Any(t.IsBasedOn) && t.IsBasedOn(type)), false)
                        .AsImplementedInterfaces()
                        .WithSingletonLifetime());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
        }
    }

    /// <summary>
    ///     Provides extension methods for registering CQRS services in an IServiceCollection.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection" /> to add the service to.</param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Registers a command handler for a given command type, with the specified service lifetime.
        /// </summary>
        /// <typeparam name="TCommand">The type of command being handled.</typeparam>
        /// <typeparam name="TCommandHandler">The type of the command handler being registered.</typeparam>
        /// <param name="serviceLifetime">
        ///     The <see cref="ServiceLifetime" /> of the command handler service.
        ///     If not specified, the default value is <see cref="ServiceLifetime.Transient" />.
        /// </param>
        /// <returns>The service collection with the command handler service added.</returns>
        /// <remarks>
        ///     This method is an extension method for <see cref="IServiceCollection" /> that adds a command handler
        ///     service to the collection. The command handler service is registered with the specified lifetime,
        ///     which determines the lifetime of the service instance. The service instance will be created when
        ///     it is first requested, and will be disposed of according to the specified lifetime.
        /// </remarks>
        public IServiceCollection RegisterCommandHandler<TCommand, TCommandHandler>(
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TCommand : ICommand
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            return serviceLifetime switch
            {
                ServiceLifetime.Singleton =>
                    serviceCollection.AddSingleton<ICommandHandler<TCommand>, TCommandHandler>(),
                ServiceLifetime.Scoped => serviceCollection.AddScoped<ICommandHandler<TCommand>, TCommandHandler>(),
                ServiceLifetime.Transient =>
                    serviceCollection.AddTransient<ICommandHandler<TCommand>, TCommandHandler>(),
                _ => serviceCollection.AddTransient<ICommandHandler<TCommand>, TCommandHandler>()
            };
        }

        /// <summary>
        ///     Registers the default command bus service, with the specified service lifetime.
        /// </summary>
        /// <param name="serviceLifetime">
        ///     The <see cref="ServiceLifetime" /> of the command bus service.
        ///     If not specified, the default value is <see cref="ServiceLifetime.Singleton" />.
        /// </param>
        /// <returns>The service collection with the command bus service added.</returns>
        /// <remarks>
        ///     This method is an extension method for <see cref="IServiceCollection" /> that adds the default
        ///     command bus service to the collection. The command bus service is registered with the specified
        ///     lifetime, which determines the lifetime of the service instance. The service instance will be
        ///     created when it is first requested, and will be disposed of according to the specified lifetime.
        /// </remarks>
        public IServiceCollection RegisterDefaultCommandBus(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            return serviceLifetime switch
            {
                ServiceLifetime.Transient => serviceCollection.AddTransient<ICommandBus, IocCommandBus>(),
                ServiceLifetime.Scoped => serviceCollection.AddScoped<ICommandBus, IocCommandBus>(),
                ServiceLifetime.Singleton => serviceCollection.AddSingleton<ICommandBus, IocCommandBus>(),
                _ => serviceCollection.AddSingleton<ICommandBus, IocCommandBus>()
            };
        }

        /// <summary>
        ///     Registers a query handler for a given query type, with the specified service lifetime.
        /// </summary>
        /// <typeparam name="TQuery">The type of query being handled.</typeparam>
        /// <typeparam name="TResult">The type of result returned by the query handler.</typeparam>
        /// <typeparam name="TQueryHandler">The type of the query handler being registered.</typeparam>
        /// <param name="serviceLifetime">
        ///     The <see cref="ServiceLifetime" /> of the query handler service.
        ///     If not specified, the default value is <see cref="ServiceLifetime.Transient" />.
        /// </param>
        /// <returns>The service collection with the query handler service added.</returns>
        /// <remarks>
        ///     This method is an extension method for <see cref="IServiceCollection" /> that adds a query handler
        ///     service to the collection. The query handler service is registered with the specified lifetime,
        ///     which determines the lifetime of the service instance. The service instance will be created when
        ///     it is first requested, and will be disposed of according to the specified lifetime.
        /// </remarks>
        public IServiceCollection RegisterQueryHandler<TQuery, TResult, TQueryHandler>(
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
            where TQuery : IQuery
            where TQueryHandler : class, IQueryHandler<TQuery, TResult>
        {
            return serviceLifetime switch
            {
                ServiceLifetime.Singleton =>
                    serviceCollection.AddSingleton<IQueryHandler<TQuery, TResult>, TQueryHandler>(),
                ServiceLifetime.Scoped => serviceCollection.AddScoped<IQueryHandler<TQuery, TResult>, TQueryHandler>(),
                ServiceLifetime.Transient =>
                    serviceCollection.AddTransient<IQueryHandler<TQuery, TResult>, TQueryHandler>(),
                _ => serviceCollection.AddTransient<IQueryHandler<TQuery, TResult>, TQueryHandler>()
            };
        }

        /// <summary>
        ///     Registers the default query processor service, with the specified service lifetime.
        /// </summary>
        /// <param name="serviceLifetime">
        ///     The <see cref="ServiceLifetime" /> of the query processor service.
        ///     If not specified, the default value is <see cref="ServiceLifetime.Singleton" />.
        /// </param>
        /// <returns>The service collection with the query processor service added.</returns>
        /// <remarks>
        ///     This method is an extension method for <see cref="IServiceCollection" /> that adds the default
        ///     query processor service to the collection. The query processor service is registered with the
        ///     specified lifetime, which determines the lifetime of the service instance. The service instance
        ///     will be created when it is first requested, and will be disposed of according to the specified
        ///     lifetime.
        /// </remarks>
        public IServiceCollection RegisterDefaultQueryProcessor(
            ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        {
            return serviceLifetime switch
            {
                ServiceLifetime.Transient => serviceCollection.AddTransient<IQueryProcessor, IocQueryProcessor>(),
                ServiceLifetime.Scoped => serviceCollection.AddScoped<IQueryProcessor, IocQueryProcessor>(),
                ServiceLifetime.Singleton => serviceCollection.AddSingleton<IQueryProcessor, IocQueryProcessor>(),
                _ => serviceCollection.AddSingleton<IQueryProcessor, IocQueryProcessor>()
            };
        }

        /// <summary>
        ///     Registers all command handlers within a given module, with a specified service lifetime.
        /// </summary>
        /// <typeparam name="TModule">The type of the module containing the command handlers to be registered.</typeparam>
        /// <param name="serviceLifetime">
        ///     The <see cref="ServiceLifetime" /> of the command handler services to be registered.
        ///     The default value is <see cref="ServiceLifetime.Transient" />.
        /// </param>
        /// <returns>The service collection with the command handler services registered.</returns>
        /// <remarks>
        ///     This method is an extension method for <see cref="IServiceCollection" /> that registers all
        ///     command handler services within a given module. The command handlers must implement
        ///     <see cref="ICommandHandler{TCommand}" />, where TCommand is the command type being handled.
        ///     The service lifetime of the command handler services is specified by the <paramref name="serviceLifetime" />
        ///     parameter.
        /// </remarks>
        public IServiceCollection RegisterModuleCommandHandlers<TModule>(
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TModule : IModule
        {
            var type = typeof(ICommandHandler<>);
            RegisterInternal<TModule>(serviceCollection, serviceLifetime, type,
                new List<Type> { typeof(ICommandDecorator<>) });
            return serviceCollection;
        }

        /// <summary>
        ///     Registers all query handlers within a given module, with a specified service lifetime.
        /// </summary>
        /// <typeparam name="TModule">The type of the module containing the query handlers to be registered.</typeparam>
        /// <param name="serviceLifetime">
        ///     The <see cref="ServiceLifetime" /> of the query handler services to be registered.
        ///     The default value is <see cref="ServiceLifetime.Transient" />.
        /// </param>
        /// <returns>The service collection with the query handler services registered.</returns>
        /// <remarks>
        ///     This method is an extension method for <see cref="IServiceCollection" /> that registers all query handler services
        ///     within a given module. The query handlers must implement <see cref="IQueryHandler{TQuery, TResult}" />, where
        ///     TQuery
        ///     is the query type being handled and TResult is the result type of the query. The service lifetime of the query
        ///     handler services is specified by the <paramref name="serviceLifetime" /> parameter.
        /// </remarks>
        public IServiceCollection RegisterModuleQueryHandlers<TModule>(
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient) where TModule : IModule
        {
            var type = typeof(IQueryHandler<,>);
            RegisterInternal<TModule>(serviceCollection, serviceLifetime, type,
                new List<Type> { typeof(IQueryDecorator<,>) });
            return serviceCollection;
        }

        /// <summary>
        ///     Registers the default CQRS infrastructure (command bus and query processor) and
        ///     optionally decorates command and query handlers with the provided decorator types.
        /// </summary>
        /// <param name="infrastructureLifetime">
        ///     The <see cref="ServiceLifetime" /> used for CQRS infrastructure services.
        ///     The default value is <see cref="ServiceLifetime.Scoped" />.
        /// </param>
        /// <param name="commandHandlerDecoratorType">
        ///     Optional decorator type to be applied to all <see cref="ICommandHandler{TCommand}" />
        ///     implementations. Must implement <see cref="ICommandDecorator{TCommand}" />.
        /// </param>
        /// <param name="queryHandlerDecoratorType">
        ///     Optional decorator type to be applied to all <see cref="IQueryHandler{TQuery, TResult}" />
        ///     implementations. Must implement <see cref="IQueryDecorator{TQuery, TResult}" />.
        /// </param>
        /// <returns>The service collection with CQRS infrastructure and optional decorators registered.</returns>
        public IServiceCollection AddCqrs(
            ServiceLifetime infrastructureLifetime = ServiceLifetime.Scoped,
            Type? commandHandlerDecoratorType = null,
            Type? queryHandlerDecoratorType = null)
        {
            serviceCollection.RegisterDefaultCommandBus(infrastructureLifetime);
            serviceCollection.RegisterDefaultQueryProcessor(infrastructureLifetime);

            if (commandHandlerDecoratorType != null &&
                serviceCollection.Any(sd => sd.ServiceType.IsGenericType &&
                                            sd.ServiceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                CqrsServiceCollectionExtensions.DecorateAllCommandHandlers(serviceCollection, commandHandlerDecoratorType);
            }

            if (queryHandlerDecoratorType != null &&
                serviceCollection.Any(sd => sd.ServiceType.IsGenericType &&
                                            sd.ServiceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            {
                CqrsServiceCollectionExtensions.DecorateAllQueryHandlers(serviceCollection, queryHandlerDecoratorType);
            }

            return serviceCollection;
        }

        /// <summary>
        ///     Registers the default CQRS infrastructure (command bus and query processor) and
        ///     decorates command and query handlers with the provided decorator types.
        /// </summary>
        /// <param name="infrastructureLifetime">
        ///     The <see cref="ServiceLifetime" /> used for CQRS infrastructure services.
        ///     The default value is <see cref="ServiceLifetime.Scoped" />.
        /// </param>
        /// <param name="commandHandlerDecoratorTypes">
        ///     An ordered sequence of decorator types to be applied to all <see cref="ICommandHandler{TCommand}" />
        ///     implementations. Each type must implement <see cref="ICommandDecorator{TCommand}" />.
        ///     <para>
        ///         Decorators are applied in the order provided in this parameter. The first type in the sequence
        ///         is registered first and therefore becomes the innermost decorator in the resulting pipeline.
        ///         Subsequent types wrap the previously registered decorator/handler.
        ///     </para>
        /// </param>
        /// <param name="queryHandlerDecoratorTypes">
        ///     An ordered sequence of decorator types to be applied to all <see cref="IQueryHandler{TQuery, TResult}" />
        ///     implementations. Each type must implement <see cref="IQueryDecorator{TQuery, TResult}" />.
        ///     <para>
        ///         Decorators are applied in the order provided in this parameter. The first type in the sequence
        ///         is registered first and therefore becomes the innermost decorator in the resulting pipeline.
        ///         Subsequent types wrap the previously registered decorator/handler.
        ///     </para>
        /// </param>
        /// <returns>
        ///     The service collection with CQRS infrastructure registered and command/query handlers decorated according
        ///     to the order of the types in <paramref name="commandHandlerDecoratorTypes" /> and
        ///     <paramref name="queryHandlerDecoratorTypes" />.
        /// </returns>
        /// <remarks>
        ///     The order of the decorator types in <paramref name="commandHandlerDecoratorTypes" /> and
        ///     <paramref name="queryHandlerDecoratorTypes" /> is significant. They are applied sequentially in the
        ///     order provided, so the first decorator type will be closest to the underlying handler, and the last
        ///     decorator type will be the outermost.
        /// </remarks>
        public IServiceCollection AddCqrs(
            ServiceLifetime infrastructureLifetime,
            IEnumerable<Type> commandHandlerDecoratorTypes,
            IEnumerable<Type> queryHandlerDecoratorTypes)
        {
            serviceCollection.RegisterDefaultCommandBus(infrastructureLifetime);
            serviceCollection.RegisterDefaultQueryProcessor(infrastructureLifetime);

            if (commandHandlerDecoratorTypes is not null &&
                serviceCollection.Any(sd => sd.ServiceType.IsGenericType &&
                                            sd.ServiceType.GetGenericTypeDefinition() == typeof(ICommandHandler<>)))
            {
                foreach (var decoratorType in commandHandlerDecoratorTypes)
                {
                    CqrsServiceCollectionExtensions.DecorateAllCommandHandlers(serviceCollection, decoratorType);
                }
            }

            if (queryHandlerDecoratorTypes is not null &&
                serviceCollection.Any(sd => sd.ServiceType.IsGenericType &&
                                            sd.ServiceType.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)))
            {
                foreach (var decoratorType in queryHandlerDecoratorTypes)
                {
                    CqrsServiceCollectionExtensions.DecorateAllQueryHandlers(serviceCollection, decoratorType);
                }
            }

            return serviceCollection;
        }
    }
}