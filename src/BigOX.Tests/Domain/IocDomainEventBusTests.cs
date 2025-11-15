using BigOX.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BigOX.Tests.Domain;

[TestClass]
public sealed class IocDomainEventBusTests
{
    [TestMethod]
    public async Task Publish_NullEvent_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        await using var provider = services.BuildServiceProvider();
        var bus = new IocDomainEventBus(provider);

        try
        {
            await bus.Publish<SampleEvent>(null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("domainEvent", ex.ParamName);
        }
    }

    [TestMethod]
    public async Task Publish_WithRegisteredHandlers_InvokesAllHandlers()
    {
        var handler1 = new CountingHandler();
        var handler2 = new CountingHandler();

        var services = new ServiceCollection();
        services.AddSingleton<IDomainEventHandler<SampleEvent>>(handler1);
        services.AddSingleton<IDomainEventHandler<SampleEvent>>(handler2);

        await using var provider = services.BuildServiceProvider();
        var bus = new IocDomainEventBus(provider);

        var evt = new SampleEvent();
        await bus.Publish(evt);

        Assert.AreEqual(1, handler1.Calls);
        Assert.AreEqual(1, handler2.Calls);
        Assert.AreSame(evt, handler1.Received.Single());
        Assert.AreSame(evt, handler2.Received.Single());
    }

    [TestMethod]
    public async Task Publish_NoHandlers_LogsWarning()
    {
        var logger = new TestLogger<IocDomainEventBus>();
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<IocDomainEventBus>>(logger);

        await using var provider = services.BuildServiceProvider();
        var bus = new IocDomainEventBus(provider);

#pragma warning disable MSTEST0049 // Flow TestContext.CancellationToken to async operations
        await bus.Publish(new SampleEvent());
#pragma warning restore MSTEST0049 // Flow TestContext.CancellationToken to async operations

        // Expect a single warning entry mentioning the event type
        Assert.HasCount(1, logger.Entries, "Expected a single log entry");
        var entry = logger.Entries[0];
        Assert.AreEqual(LogLevel.Warning, entry.Level);
        StringAssert.Contains(entry.Message, typeof(SampleEvent).FullName);

        // Also verify structured state contains EventType when available
        if (entry.State is { } state && state.TryGetValue("EventType", out var value))
        {
            Assert.AreEqual(typeof(SampleEvent).FullName, value);
        }
    }

    // Simple domain event for testing
    private sealed class SampleEvent : IDomainEvent;

    // Test handler that counts invocations
    private sealed class CountingHandler : IDomainEventHandler<SampleEvent>
    {
        public int Calls { get; private set; }
        public List<SampleEvent> Received { get; } = new();

        public Task Handle(SampleEvent @event, CancellationToken cancellationToken = default)
        {
            Calls++;
            Received.Add(@event);
            return Task.CompletedTask;
        }
    }

    // In-memory logger to capture logs from the bus
    private sealed class TestLogger<T> : ILogger<T>
    {
        public readonly List<Entry> Entries = new();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Dictionary<string, object?>? kv = null;
            if (state is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                kv = new Dictionary<string, object?>();
                foreach (var p in pairs)
                {
                    kv[p.Key] = p.Value;
                }
            }

            Entries.Add(new Entry(logLevel, eventId, message, exception, kv));
        }

        public readonly record struct Entry(
            LogLevel Level,
            // ReSharper disable once NotAccessedPositionalProperty.Local
            EventId EventId,
            string Message,
            Exception? Exception,
            Dictionary<string, object?>? State);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}