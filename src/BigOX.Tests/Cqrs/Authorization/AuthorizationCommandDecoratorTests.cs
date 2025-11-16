using System.Security;
using BigOX.Cqrs;
using BigOX.Cqrs.Authorization;
using BigOX.Security;

namespace BigOX.Tests.Cqrs.Authorization;

[TestClass]
public sealed class AuthorizationCommandDecoratorTests
{
    [TestMethod]
    public async Task Handle_AuthorizesAndInvokesInner_OnSuccess()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: true);
        var sut = new AuthorizationCommandDecorator<TestCommand>(handler, auth);

        await sut.Handle(new TestCommand("ok"));

        Assert.AreEqual(1, handler.CallCount);
        Assert.AreEqual(1, auth.AuthorizeCallCount);
        Assert.AreSame(handler.LastCommand, auth.LastArgs);
    }

    [TestMethod]
    public async Task Handle_AuthorizationFails_ThrowsSecurityException_DoesNotInvokeInner()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: false);
        var sut = new AuthorizationCommandDecorator<TestCommand>(handler, auth);

        try
        {
            await sut.Handle(new TestCommand("denied"));
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
    public async Task Handle_NullCommand_ThrowsArgumentNullException_DoesNotInvokeInner()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: true);
        var sut = new AuthorizationCommandDecorator<TestCommand>(handler, auth);

        try
        {
            await sut.Handle(null!);
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
    public async Task Handle_PassesCancellationToken_ToAuthorizationManagerAndInner()
    {
        var handler = new CapturingHandler();
        var auth = new StubAuthorizationManager(success: true);
        var sut = new AuthorizationCommandDecorator<TestCommand>(handler, auth);

        using var cts = new CancellationTokenSource();
        await sut.Handle(new TestCommand("ok"), cts.Token);

        Assert.AreEqual(cts.Token, handler.ObservedToken);
        Assert.AreEqual(cts.Token, auth.ObservedToken);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestCommand(string Name) : ICommand;

    private sealed class CapturingHandler : ICommandHandler<TestCommand>
    {
        public int CallCount { get; private set; }
        public TestCommand? LastCommand { get; private set; }
        public CancellationToken ObservedToken { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastCommand = command;
            ObservedToken = cancellationToken;
            return Task.CompletedTask;
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
