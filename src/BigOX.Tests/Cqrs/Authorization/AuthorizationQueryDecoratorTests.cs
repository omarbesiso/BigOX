using System.Security;
using BigOX.Cqrs;
using BigOX.Cqrs.Authorization;
using BigOX.Security;

namespace BigOX.Tests.Cqrs.Authorization;

[TestClass]
public sealed class AuthorizationQueryDecoratorTests
{
    [TestMethod]
    public async Task Read_AuthorizesAndInvokesInner_OnSuccess()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: true);
        var sut = new AuthorizationQueryDecorator<TestQuery, int>(handler, auth);

        var result = await sut.Read(new TestQuery("q"));

        Assert.AreEqual(42, result);
        Assert.AreEqual(1, handler.CallCount);
        Assert.AreEqual(1, auth.AuthorizeCallCount);
        Assert.AreSame(handler.LastQuery, auth.LastArgs);
    }

    [TestMethod]
    public async Task Read_AuthorizationFails_ThrowsSecurityException_DoesNotInvokeInner()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: false);
        var sut = new AuthorizationQueryDecorator<TestQuery, int>(handler, auth);

        try
        {
            _ = await sut.Read(new TestQuery("denied"));
            Assert.Fail("Expected SecurityException");
        }
        catch (SecurityException)
        {
            // expected
        }

        Assert.AreEqual(0, handler.CallCount);
        Assert.AreEqual(1, auth.AuthorizeCallCount);
    }

    [TestMethod]
    public async Task Read_NullQuery_ThrowsArgumentNullException_DoesNotInvokeInner()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: true);
        var sut = new AuthorizationQueryDecorator<TestQuery, int>(handler, auth);

        try
        {
            _ = await sut.Read(null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // expected
        }

        Assert.AreEqual(0, handler.CallCount);
        Assert.AreEqual(0, auth.AuthorizeCallCount);
    }

    [TestMethod]
    public async Task Read_PassesCancellationToken_ToAuthorizationManagerAndInner()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: true);
        var sut = new AuthorizationQueryDecorator<TestQuery, int>(handler, auth);

        using var cts = new CancellationTokenSource();
        _ = await sut.Read(new TestQuery("q"), cts.Token);

        Assert.AreEqual(cts.Token, handler.ObservedToken);
        Assert.AreEqual(cts.Token, auth.ObservedToken);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestQuery(string Name) : IQuery;

    private sealed class CapturingHandler : IQueryHandler<TestQuery, int>
    {
        public int CallCount { get; private set; }
        public TestQuery? LastQuery { get; private set; }
        public CancellationToken ObservedToken { get; private set; }

        public Task<int> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastQuery = query;
            ObservedToken = cancellationToken;
            return Task.FromResult(42);
        }
    }

    private sealed class StubAuthorizationManager(bool success) : IAuthorizationManager
    {
        public int AuthorizeCallCount { get; private set; }
        public object? LastArgs { get; private set; }
        public CancellationToken ObservedToken { get; private set; }

        public ValueTask<AuthorizationEvaluationResult> EvaluateAsync<TAuthorizationArgs>(TAuthorizationArgs authorizationArgs, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Decorator calls AuthorizeAsync directly in tests.");
        }

        public ValueTask AuthorizeAsync<TAuthorizationArgs>(TAuthorizationArgs authorizationArgs, CancellationToken cancellationToken = default)
        {
            AuthorizeCallCount++;
            LastArgs = authorizationArgs;
            ObservedToken = cancellationToken;
            if (success)
            {
                return ValueTask.CompletedTask;
            }
            throw new SecurityException("Authorization failed");
        }
    }
}
