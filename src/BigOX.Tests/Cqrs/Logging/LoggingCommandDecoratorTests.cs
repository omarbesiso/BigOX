using BigOX.Cqrs;
using BigOX.Cqrs.Logging;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace BigOX.Tests.Cqrs.Logging;

[TestClass]
public class LoggingCommandDecoratorTests
{
    private static async Task<TException> AssertThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException ex)
        {
            return ex;
        }

        Assert.Fail($"Expected exception of type {typeof(TException).Name} was not thrown.");
        throw new InvalidOperationException("Unreachable");
    }

    [TestMethod]
    public async Task Handle_LogsStartAndExecuted_OnSuccess()
    {
        var inner = new SucceedingHandler();
        var logger = new CapturingLogger<LoggingCommandDecorator<TestCommand>>();
        var sut = new LoggingCommandDecorator<TestCommand>(inner, logger);

        await sut.Handle(new TestCommand());

        Assert.AreEqual(1, inner.CallCount, "Inner handler should be called once");

        // Start (2001)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 2001 && e.Message.Contains("Start executing command 'TestCommand'")));
        // Executed (2003)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 2003 && e.Message.Contains("Executed command 'TestCommand' in")));
        // No error
        Assert.IsFalse(logger.Entries.Any(e => e.EventId.Id == 2002));
    }

    [TestMethod]
    public async Task Handle_LogsErrorAndRethrows_OnFailure()
    {
        var inner = new ThrowingHandler();
        var logger = new CapturingLogger<LoggingCommandDecorator<TestCommand>>();
        var sut = new LoggingCommandDecorator<TestCommand>(inner, logger);

        var ex = await AssertThrowsAsync<InvalidOperationException>(async () =>
        {
            await sut.Handle(new TestCommand());
        });
        Assert.AreEqual("Boom", ex.Message);

        // Start (2001)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 2001 && e.Message.Contains("Start executing command 'TestCommand'")));
        // Error (2002)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 2002 && e.Message.Contains("Exception thrown while executing command 'TestCommand'")));
        // Executed (2003) still logged in finally
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 2003 && e.Message.Contains("Executed command 'TestCommand' in")));
    }

    [TestMethod]
    public async Task Handle_ThrowsArgumentNull_WhenCommandIsNull()
    {
        var inner = new SucceedingHandler();
        var logger = new CapturingLogger<LoggingCommandDecorator<TestCommand>>();
        var sut = new LoggingCommandDecorator<TestCommand>(inner, logger);

        await AssertThrowsAsync<ArgumentNullException>(async () => { await sut.Handle(null!); });

        // No logs should be written because Guard.NotNull throws before logging
        Assert.IsEmpty(logger.Entries);
        Assert.AreEqual(0, inner.CallCount);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationToken_ToInnerHandler()
    {
        var inner = new SucceedingHandler();
        var logger = new CapturingLogger<LoggingCommandDecorator<TestCommand>>();
        var sut = new LoggingCommandDecorator<TestCommand>(inner, logger);

        using var cts = new CancellationTokenSource();
        await sut.Handle(new TestCommand(), cts.Token);

        Assert.IsTrue(inner.ObservedToken.HasValue);
        Assert.AreEqual(cts.Token, inner.ObservedToken!.Value);
    }

    private sealed class TestCommand : ICommand
    {
    }

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _entries = [];
        public IReadOnlyList<LogEntry> Entries => _entries;

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
        public IDisposable BeginScope<TState>(TState state)
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
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

    private sealed class SucceedingHandler : ICommandHandler<TestCommand>
    {
        public int CallCount { get; private set; }
        public CancellationToken? ObservedToken { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            CallCount++;
            ObservedToken = cancellationToken;
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Boom");
        }
    }
}