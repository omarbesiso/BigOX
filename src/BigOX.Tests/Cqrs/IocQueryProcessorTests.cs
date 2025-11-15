using BigOX.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Tests.Cqrs;

[TestClass]
public sealed class IocQueryProcessorTests
{
    [TestMethod]
    public async Task ProcessQuery_NullQuery_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        await using var provider = services.BuildServiceProvider();
        var processor = new IocQueryProcessor(provider);

        try
        {
            await processor.ProcessQuery<TestQuery, string>(null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("query", ex.ParamName);
        }
    }

    [TestMethod]
    public async Task ProcessQuery_WithRegisteredHandler_InvokesHandler_AndReturnsResult()
    {
        var handler = new CountingHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IQueryHandler<TestQuery, string>>(handler);

        await using var provider = services.BuildServiceProvider();
        var processor = new IocQueryProcessor(provider);

        var query = new TestQuery("abc");
        var result = await processor.ProcessQuery<TestQuery, string>(query);

        Assert.AreEqual(1, handler.Calls);
        Assert.AreSame(query, handler.Received.Single());
        Assert.AreEqual($"len:{query.Payload.Length}", result);
    }

    [TestMethod]
    public async Task ProcessQuery_PassesCancellationToken_ToHandler()
    {
        var handler = new CountingHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IQueryHandler<TestQuery, string>>(handler);

        await using var provider = services.BuildServiceProvider();
        var processor = new IocQueryProcessor(provider);

        using var cts = new CancellationTokenSource();
        var query = new TestQuery("xyz");

        _ = await processor.ProcessQuery<TestQuery, string>(query, cts.Token);

        Assert.AreEqual(cts.Token, handler.LastToken);
    }

    [TestMethod]
    public async Task ProcessQuery_NoHandlerRegistered_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        await using var provider = services.BuildServiceProvider();
        var processor = new IocQueryProcessor(provider);

        try
        {
            await processor.ProcessQuery<TestQuery, string>(new TestQuery("abc"));
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }
    }

    private sealed record TestQuery(string Payload) : IQuery;

    private sealed class CountingHandler : IQueryHandler<TestQuery, string>
    {
        public int Calls { get; private set; }
        public List<TestQuery> Received { get; } = new();
        public CancellationToken LastToken { get; private set; }

        public Task<string> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            Calls++;
            Received.Add(query);
            LastToken = cancellationToken;
            return Task.FromResult($"len:{query.Payload.Length}");
        }
    }
}