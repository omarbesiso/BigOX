using BigOX.Cqrs;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Tests.Cqrs;

[TestClass]
public sealed class IocCommandBusTests
{
    [TestMethod]
    public async Task Send_NullCommand_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        await using var provider = services.BuildServiceProvider();
        var bus = new IocCommandBus(provider);

        try
        {
            await bus.Send<TestCommand>(null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("command", ex.ParamName);
        }
    }

    [TestMethod]
    public async Task Send_WithRegisteredHandler_InvokesHandler()
    {
        var handler = new CountingHandler();
        var services = new ServiceCollection();
        services.AddSingleton<ICommandHandler<TestCommand>>(handler);

        await using var provider = services.BuildServiceProvider();
        var bus = new IocCommandBus(provider);

        var cmd = new TestCommand();
        await bus.Send(cmd);

        Assert.AreEqual(1, handler.Calls);
        Assert.AreSame(cmd, handler.Received.Single());
    }

    private sealed record TestCommand : ICommand;

    private sealed class CountingHandler : ICommandHandler<TestCommand>
    {
        public int Calls { get; private set; }
        public List<TestCommand> Received { get; } = new();

        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            Calls++;
            Received.Add(command);
            return Task.CompletedTask;
        }
    }
}