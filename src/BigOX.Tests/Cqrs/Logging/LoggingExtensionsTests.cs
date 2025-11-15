using BigOX.Cqrs;
using BigOX.Cqrs.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace BigOX.Tests.Cqrs.Logging;

[TestClass]
public class LoggingExtensionsTests
{
    [TestMethod]
    public async Task DecorateCommandHandlerWithLogging_Wraps_Handler_And_Logs()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestCommandHandler>();
        services.AddSingleton<ICommandHandler<TestCommand>>(sp => sp.GetRequiredService<TestCommandHandler>());

        var cmdLogger = new CapturingLogger<LoggingCommandDecorator<TestCommand>>();
        services.AddSingleton<ILogger<LoggingCommandDecorator<TestCommand>>>(cmdLogger);

        // Act: apply logging decoration
        var chained = services.DecorateCommandHandlerWithLogging<TestCommand>();
        Assert.AreSame(services, chained, "Should return same service collection for chaining");

        await using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<ICommandHandler<TestCommand>>();

        // Execute
        await handler.Handle(new TestCommand());

        // Assert underlying handler was called
        var inner = provider.GetRequiredService<TestCommandHandler>();
        Assert.AreEqual(1, inner.Calls);

        // Assert logs: 2001 (start) and 2003 (executed), no 2002 (error)
        Assert.IsTrue(cmdLogger.Entries.Any(e =>
            e.EventId.Id == 2001 && e.Message.Contains("Start executing command 'TestCommand'")));
        Assert.IsTrue(cmdLogger.Entries.Any(e =>
            e.EventId.Id == 2003 && e.Message.Contains("Executed command 'TestCommand' in")));
        Assert.IsFalse(cmdLogger.Entries.Any(e => e.EventId.Id == 2002));
    }

    [TestMethod]
    public async Task DecorateQueryHandlerWithLogging_Wraps_Handler_And_Logs()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestQueryHandler>();
        services.AddSingleton<IQueryHandler<TestQuery, int>>(sp => sp.GetRequiredService<TestQueryHandler>());

        var qryLogger = new CapturingLogger<LoggingQueryDecorator<TestQuery, int>>();
        services.AddSingleton<ILogger<LoggingQueryDecorator<TestQuery, int>>>(qryLogger);

        // Act: apply logging decoration
        var chained = services.DecorateQueryHandlerWithLogging<TestQuery, int>();
        Assert.AreSame(services, chained);

        await using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IQueryHandler<TestQuery, int>>();

        // Execute
        var result = await handler.Read(new TestQuery());

        // Assert underlying handler was called and value flowed through
        var inner = provider.GetRequiredService<TestQueryHandler>();
        Assert.AreEqual(1, inner.Calls);
        Assert.AreEqual(7, result);

        // Assert logs: 1001 (start) and 1003 (executed), no 1002 (error)
        Assert.IsTrue(qryLogger.Entries.Any(e =>
            e.EventId.Id == 1001 && e.Message.Contains("Start reading query 'TestQuery'")));
        Assert.IsTrue(qryLogger.Entries.Any(e =>
            e.EventId.Id == 1003 && e.Message.Contains("Executed query 'TestQuery' in")));
        Assert.IsFalse(qryLogger.Entries.Any(e => e.EventId.Id == 1002));
    }

    private sealed class TestCommand : ICommand;

    private sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public int Calls { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestQuery : IQuery;

    private sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public int Calls { get; private set; }

        public Task<int> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(7);
        }
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _entries = [];
        public IReadOnlyList<LogEntry> Entries => _entries;

#pragma warning disable CS8633 // Nullability in constraints mismatch with implicit interface method
        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }
#pragma warning restore CS8633
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            _entries.Add(new LogEntry
            {
                LogLevel = logLevel,
                EventId = eventId,
                Message = message,
                Exception = exception,
                Category = typeof(T).FullName!
            });
        }

        public sealed class LogEntry
        {
            public LogLevel LogLevel { get; init; }
            public EventId EventId { get; init; }
            public string Message { get; init; } = string.Empty;
            public Exception? Exception { get; init; }
            public string Category { get; init; } = typeof(T).FullName!;
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}