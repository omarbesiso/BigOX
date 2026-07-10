using BigOX.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BigOX.Domain;

/// <summary>
///     A default implementation of the <see cref="IDomainEventBus" /> using IOC to deliver published events to the
///     relevant handlers.
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="IocDomainEventBus" /> class.
/// </remarks>
/// <param name="serviceProvider">The service provider to resolve event handlers.</param>
internal sealed class IocDomainEventBus(IServiceProvider serviceProvider) : IDomainEventBus
{
    /// <summary>
    ///     Publishes the specified domain event to all registered event handlers.
    ///     Handlers are resolved from the service provider and invoked sequentially; an exception thrown by a handler
    ///     propagates to the caller and prevents the remaining handlers from running.
    ///     If no handlers are registered, this is treated as a no-op and a warning is logged (when a logger is
    ///     available).
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="domainEvent" /> is <see langword="null" />.
    /// </exception>
    public async Task Publish<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
        where TDomainEvent : IDomainEvent
    {
        Guard.NotNull(domainEvent);

        var handlers = serviceProvider.GetServices<IDomainEventHandler<TDomainEvent>>();

        var anyHandler = false;
        foreach (var handler in handlers)
        {
            anyHandler = true;
            await handler.Handle(domainEvent, cancellationToken).ConfigureAwait(false);
        }

        if (!anyHandler)
        {
            var logger = serviceProvider.GetService<ILogger<IocDomainEventBus>>();
            logger?.LogWarning("No registered handlers found for domain event type {EventType}",
                typeof(TDomainEvent).FullName);
        }
    }
}