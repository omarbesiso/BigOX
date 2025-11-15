using System.Transactions;
using BigOX.Cqrs;
using BigOX.Cqrs.Transactions;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Tests.Cqrs.Transactions;

[TestClass]
public sealed class TransactionExtensionsTests
{
    [TestMethod]
    public void DecorateCommandHandlerWithTransactions_Returns_Same_ServiceCollection()
    {
        var services = new ServiceCollection();
        var chained = services.DecorateCommandHandlerWithTransactions<TestCommand>();
        Assert.AreSame(services, chained);
    }

    [TestMethod]
    public async Task DecorateCommandHandlerWithTransactions_Wraps_Handler_And_Commits_OnSuccess()
    {
        var resource = new RecordingResource();
        var services = new ServiceCollection();
        services.AddSingleton(resource);
        services.AddSingleton<SucceedingHandler>();
        services.AddSingleton<ICommandHandler<TestCommand>>(sp => sp.GetRequiredService<SucceedingHandler>());

        services.DecorateCommandHandlerWithTransactions<TestCommand>();

        await using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<ICommandHandler<TestCommand>>();

        Assert.IsInstanceOfType(handler, typeof(DefaultTransactionCommandDecorator<TestCommand>));

        await handler.Handle(new TestCommand());

        var inner = provider.GetRequiredService<SucceedingHandler>();
        Assert.AreEqual(1, inner.Calls);
        Assert.IsTrue(inner.AmbientTransactionSeen, "Ambient Transaction should be available within handler");
        Assert.IsTrue(resource.Committed, "Transaction should commit on success");
        Assert.IsFalse(resource.RolledBack, "Transaction should not roll back on success");
    }

    [TestMethod]
    public async Task DecorateCommandHandlerWithTransactions_Wraps_Handler_And_RollsBack_OnFailure()
    {
        var resource = new RecordingResource();
        var services = new ServiceCollection();
        services.AddSingleton(resource);
        services.AddSingleton<ThrowingHandler>();
        services.AddSingleton<ICommandHandler<TestCommand>>(sp => sp.GetRequiredService<ThrowingHandler>());

        services.DecorateCommandHandlerWithTransactions<TestCommand>();

        await using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<ICommandHandler<TestCommand>>();

        Assert.IsInstanceOfType(handler, typeof(DefaultTransactionCommandDecorator<TestCommand>));

        try
        {
            await handler.Handle(new TestCommand());
            Assert.Fail("Expected exception");
        }
        catch (InvalidOperationException ex)
        {
            Assert.AreEqual("Boom", ex.Message);
        }

        var inner = provider.GetRequiredService<ThrowingHandler>();
        Assert.IsTrue(inner.AmbientTransactionSeen, "Ambient Transaction should be available within handler");
        Assert.IsFalse(resource.Committed, "Transaction should not commit on failure");
        Assert.IsTrue(resource.RolledBack, "Transaction should roll back on failure");
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

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;
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