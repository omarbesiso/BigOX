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
    ///     By default, if no handlers are registered, this is treated as a no-op and a debug log is emitted.
    /// </summary>
    /// <typeparam name="TDomainEvent">The type of the domain event.</typeparam>
    /// <param name="domainEvent">The domain event to publish.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when configured to throw and no handlers are found for the event
    ///     type.
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