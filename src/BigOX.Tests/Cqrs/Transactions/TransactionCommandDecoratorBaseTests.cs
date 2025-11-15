using System.Transactions;
using BigOX.Cqrs;
using BigOX.Cqrs.Transactions;

namespace BigOX.Tests.Cqrs.Transactions;

[TestClass]
public sealed class TransactionCommandDecoratorBaseTests
{
    [TestMethod]
    public async Task Handle_CreatesAndCompletes_TransactionScope_OnSuccess()
    {
        var inner = new SucceedingHandler();
        var sut = new TestDecorator(inner);

        await sut.Handle(new TestCommand());

        Assert.AreEqual(1, inner.Calls);
        Assert.IsTrue(sut.ScopeCreated, "TransactionScope should be created");
        Assert.IsTrue(sut.ScopeCompleted, "TransactionScope should be completed on success");
    }

    [TestMethod]
    public async Task Handle_DoesNotComplete_WhenInnerThrows_Rethrows()
    {
        var inner = new ThrowingHandler();
        var sut = new TestDecorator(inner);

        try
        {
            await sut.Handle(new TestCommand());
            Assert.Fail("Expected exception");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Boom", ex.Message);
        }

        Assert.IsTrue(sut.ScopeCreated, "TransactionScope should be created");
        Assert.IsFalse(sut.ScopeCompleted, "TransactionScope should not be completed on failure");
    }

    [TestMethod]
    public async Task Handle_ThrowsArgumentNull_WhenCommandIsNull()
    {
        var inner = new SucceedingHandler();
        var sut = new TestDecorator(inner);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await sut.Handle(null!));
        Assert.AreEqual(0, inner.Calls);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationToken_ToInner()
    {
        var inner = new SucceedingHandler();
        var sut = new TestDecorator(inner);

        using var cts = new CancellationTokenSource();
        await sut.Handle(new TestCommand(), cts.Token);

        Assert.IsTrue(inner.ObservedToken.HasValue);
        Assert.AreEqual(cts.Token, inner.ObservedToken!.Value);
    }

    [TestMethod]
    public async Task Can_Override_TransactionOptions_And_ScopeOptions()
    {
        var inner = new SucceedingHandler();
        var sut = new TestDecorator(inner)
        {
            TransactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.Serializable,
                Timeout = TimeSpan.FromSeconds(5)
            },
            TransactionScopeOption = TransactionScopeOption.RequiresNew,
            TransactionScopeAsyncFlowOption = TransactionScopeAsyncFlowOption.Suppress
        };

        await sut.Handle(new TestCommand());

        Assert.AreEqual(IsolationLevel.Serializable, sut.LastScopeOptions?.IsolationLevel);
        Assert.AreEqual(TimeSpan.FromSeconds(5), sut.LastScopeOptions?.Timeout);
        Assert.AreEqual(TransactionScopeOption.RequiresNew, sut.LastScopeScopeOption);
        Assert.AreEqual(TransactionScopeAsyncFlowOption.Suppress, sut.LastScopeAsyncFlowOption);
    }

    private sealed class TestDecorator(ICommandHandler<TestCommand> decorated)
        : TransactionCommandDecoratorBase<TestCommand>(decorated)
    {
        public bool ScopeCreated { get; private set; }
        public bool ScopeCompleted { get; private set; }

        public TransactionOptions? LastScopeOptions { get; private set; }
        public TransactionScopeOption? LastScopeScopeOption { get; private set; }
        public TransactionScopeAsyncFlowOption? LastScopeAsyncFlowOption { get; private set; }

        public new TransactionOptions TransactionOptions
        {
            get => base.TransactionOptions;
            init => base.TransactionOptions = value;
        }

        public new TransactionScopeOption TransactionScopeOption
        {
            get => base.TransactionScopeOption;
            init => base.TransactionScopeOption = value;
        }

        public new TransactionScopeAsyncFlowOption TransactionScopeAsyncFlowOption
        {
            get => base.TransactionScopeAsyncFlowOption;
            init => base.TransactionScopeAsyncFlowOption = value;
        }

        public new async Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            try
            {
                ScopeCreated = true;
                LastScopeOptions = TransactionOptions;
                LastScopeScopeOption = TransactionScopeOption;
                LastScopeAsyncFlowOption = TransactionScopeAsyncFlowOption;
                await base.Handle(command, cancellationToken);
                ScopeCompleted = true; // only set if base didn't throw
            }
            catch
            {
                // base.Handle throws after not completing scope
                ScopeCompleted = false;
                throw;
            }
        }
    }

    private sealed class TestCommand : ICommand
    {
    }

    private sealed class SucceedingHandler : ICommandHandler<TestCommand>
    {
        public int Calls { get; private set; }
        public CancellationToken? ObservedToken { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;
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