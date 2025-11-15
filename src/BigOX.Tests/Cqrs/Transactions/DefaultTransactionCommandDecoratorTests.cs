using System.Transactions;
using BigOX.Cqrs;
using BigOX.Cqrs.Transactions;

namespace BigOX.Tests.Cqrs.Transactions;

[TestClass]
public sealed class DefaultTransactionCommandDecoratorTests
{
    [TestMethod]
    public async Task Handle_CreatesAmbientTransaction_And_Commits_OnSuccess()
    {
        var resource = new RecordingResource();
        var inner = new SucceedingHandler(resource);
        var sut = new DefaultTransactionCommandDecorator<TestCommand>(inner);

        await sut.Handle(new TestCommand());

        Assert.AreEqual(1, inner.Calls);
        Assert.IsTrue(inner.AmbientTransactionSeen, "Ambient Transaction should be available within handler");
        Assert.IsTrue(resource.Committed, "Transaction should commit on success");
        Assert.IsFalse(resource.RolledBack, "Transaction should not roll back on success");
    }

    [TestMethod]
    public async Task Handle_CreatesAmbientTransaction_And_RollsBack_OnFailure()
    {
        var resource = new RecordingResource();
        var inner = new ThrowingHandler(resource);
        var sut = new DefaultTransactionCommandDecorator<TestCommand>(inner);

        try
        {
            await sut.Handle(new TestCommand());
            Assert.Fail("Expected exception");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Boom", ex.Message);
        }

        Assert.IsTrue(inner.AmbientTransactionSeen, "Ambient Transaction should be available within handler");
        Assert.IsFalse(resource.Committed, "Transaction should not commit on failure");
        Assert.IsTrue(resource.RolledBack, "Transaction should roll back on failure");
    }

    [TestMethod]
    public async Task Handle_ThrowsArgumentNull_WhenCommandIsNull()
    {
        var resource = new RecordingResource();
        var inner = new SucceedingHandler(resource);
        var sut = new DefaultTransactionCommandDecorator<TestCommand>(inner);

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await sut.Handle(null!));
        Assert.AreEqual(0, inner.Calls);
    }

    [TestMethod]
    public async Task Handle_PassesCancellationToken_ToInner()
    {
        var resource = new RecordingResource();
        var inner = new SucceedingHandler(resource);
        var sut = new DefaultTransactionCommandDecorator<TestCommand>(inner);

        using var cts = new CancellationTokenSource();
        await sut.Handle(new TestCommand(), cts.Token);

        Assert.IsTrue(inner.ObservedToken.HasValue);
        Assert.AreEqual(cts.Token, inner.ObservedToken!.Value);
    }

    private sealed class TestCommand : ICommand
    {
    }

    private sealed class RecordingResource : IEnlistmentNotification
    {
        public bool Committed { get; private set; }
        public bool RolledBack { get; private set; }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            Committed = true;
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            RolledBack = true;
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }
    }

    private sealed class SucceedingHandler(RecordingResource resource) : ICommandHandler<TestCommand>
    {
        public int Calls { get; private set; }
        public bool AmbientTransactionSeen { get; private set; }
        public CancellationToken? ObservedToken { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;
            ObservedToken = cancellationToken;
            var tx = Transaction.Current;
            AmbientTransactionSeen = tx is not null;
            tx?.EnlistVolatile(resource, EnlistmentOptions.None);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler(RecordingResource resource) : ICommandHandler<TestCommand>
    {
        public bool AmbientTransactionSeen { get; private set; }

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            var tx = Transaction.Current;
            AmbientTransactionSeen = tx is not null;
            tx?.EnlistVolatile(resource, EnlistmentOptions.None);
            throw new InvalidOperationException("Boom");
        }
    }
}