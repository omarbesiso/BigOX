using BigOX.Cqrs;
using BigOX.Cqrs.Logging;
using Microsoft.Extensions.Logging;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace BigOX.Tests.Cqrs.Logging;

[TestClass]
public class LoggingQueryDecoratorTests
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
    public async Task Read_LogsStartAndExecuted_OnSuccess()
    {
        var inner = new SucceedingHandler();
        var logger = new CapturingLogger<LoggingQueryDecorator<TestQuery, int>>();
        var sut = new LoggingQueryDecorator<TestQuery, int>(inner, logger);

        var result = await sut.Read(new TestQuery());

        Assert.AreEqual(42, result, "Inner handler result should flow through");
        Assert.AreEqual(1, inner.CallCount, "Inner handler should be called once");

        // Start (1001)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 1001 && e.Message.Contains("Start reading query 'TestQuery'")));
        // Executed (1003)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 1003 && e.Message.Contains("Executed query 'TestQuery' in")));
        // No error
        Assert.IsFalse(logger.Entries.Any(e => e.EventId.Id == 1002));
    }

    [TestMethod]
    public async Task Read_LogsErrorAndRethrows_OnFailure()
    {
        var inner = new ThrowingHandler();
        var logger = new CapturingLogger<LoggingQueryDecorator<TestQuery, int>>();
        var sut = new LoggingQueryDecorator<TestQuery, int>(inner, logger);

        var ex = await AssertThrowsAsync<InvalidOperationException>(async () =>
        {
            _ = await sut.Read(new TestQuery());
        });
        Assert.AreEqual("Boom", ex.Message);

        // Start (1001)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 1001 && e.Message.Contains("Start reading query 'TestQuery'")));
        // Error (1002)
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 1002 && e.Message.Contains("Exception thrown while reading query 'TestQuery'")));
        // Executed (1003) still logged in finally
        Assert.IsTrue(logger.Entries.Any(e =>
            e.EventId.Id == 1003 && e.Message.Contains("Executed query 'TestQuery' in")));
    }

    [TestMethod]
    public async Task Read_ThrowsArgumentNull_WhenQueryIsNull()
    {
        var inner = new SucceedingHandler();
        var logger = new CapturingLogger<LoggingQueryDecorator<TestQuery, int>>();
        var sut = new LoggingQueryDecorator<TestQuery, int>(inner, logger);

        await AssertThrowsAsync<ArgumentNullException>(async () => { _ = await sut.Read(null!); });

        // No logs should be written because Guard.NotNull throws before logging
        Assert.IsEmpty(logger.Entries);
        Assert.AreEqual(0, inner.CallCount);
    }

    [TestMethod]
    public async Task Read_PassesCancellationToken_ToInnerHandler()
    {
        var inner = new SucceedingHandler();
        var logger = new CapturingLogger<LoggingQueryDecorator<TestQuery, int>>();
        var sut = new LoggingQueryDecorator<TestQuery, int>(inner, logger);

        using var cts = new CancellationTokenSource();
        _ = await sut.Read(new TestQuery(), cts.Token);

        Assert.IsTrue(inner.ObservedToken.HasValue);
        Assert.AreEqual(cts.Token, inner.ObservedToken!.Value);
    }

    private sealed class TestQuery : IQuery;

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

    private sealed class SucceedingHandler : IQueryHandler<TestQuery, int>
    {
        public int CallCount { get; private set; }
        public CancellationToken? ObservedToken { get; private set; }

        public Task<int> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            CallCount++;
            ObservedToken = cancellationToken;
            return Task.FromResult(42);
        }
    }

    private sealed class ThrowingHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Boom");
        }
    }
}