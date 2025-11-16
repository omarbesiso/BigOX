using BigOX.Cqrs;
using BigOX.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedType.Local

namespace BigOX.Tests.Cqrs;

[TestClass]
public class CqrsServiceCollectionExtensionsTests
{
    private static ServiceProvider Build(ServiceCollection services)
    {
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void DecorateAllCommandHandlers_Throws_When_Type_Does_Not_Implement_ICommandDecorator()
    {
        var services = new ServiceCollection();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>();

        Assert.ThrowsExactly<ArgumentException>(() =>
            CqrsServiceCollectionExtensions.DecorateAllCommandHandlers(services, typeof(InvalidCommandDecorator<>)));
    }

    [TestMethod]
    public void DecorateAllQueryHandlers_Throws_When_Type_Does_Not_Implement_IQueryDecorator()
    {
        var services = new ServiceCollection();
        services.RegisterQueryHandler<TestQuery, int, TestQueryHandler>();

        Assert.ThrowsExactly<ArgumentException>(() =>
            CqrsServiceCollectionExtensions.DecorateAllQueryHandlers(services, typeof(InvalidQueryDecorator<,>)));
    }

    [TestMethod]
    public async Task DecorateAllCommandHandlers_Wraps_Handler()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>();

        CqrsServiceCollectionExtensions.DecorateAllCommandHandlers(services, typeof(CommandLoggingDecorator<>));

        await using var provider = Build(services);
        var handler = provider.GetRequiredService<ICommandHandler<TestCommand>>();
        Assert.IsInstanceOfType(handler, typeof(CommandLoggingDecorator<TestCommand>));

        var probe = provider.GetRequiredService<Probe>();
        await handler.Handle(new TestCommand());
        Assert.AreEqual(1, probe.CommandCalls);
        Assert.AreEqual(1, probe.InnerCommandCalls);
    }

    [TestMethod]
    public async Task DecorateAllQueryHandlers_Wraps_Handler()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterQueryHandler<TestQuery, int, TestQueryHandler>();

        CqrsServiceCollectionExtensions.DecorateAllQueryHandlers(services, typeof(QueryLoggingDecorator<,>));

        await using var provider = Build(services);
        var handler = provider.GetRequiredService<IQueryHandler<TestQuery, int>>();
        Assert.IsInstanceOfType(handler, typeof(QueryLoggingDecorator<TestQuery, int>));

        var probe = provider.GetRequiredService<Probe>();
        var result = await handler.Read(new TestQuery());
        Assert.AreEqual(1, result);
        Assert.AreEqual(1, probe.QueryCalls);
        Assert.AreEqual(1, probe.InnerQueryCalls);
    }

    [TestMethod]
    public async Task RegisterDefaultCommandBus_Resolves_And_Sends()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>();
        services.RegisterDefaultCommandBus();

        await using var provider = Build(services);
        var bus = provider.GetRequiredService<ICommandBus>();
        Assert.IsInstanceOfType(bus, typeof(IocCommandBus));

        await bus.Send(new TestCommand());
        var probe = provider.GetRequiredService<Probe>();
        Assert.AreEqual(1, probe.InnerCommandCalls);
    }

    [TestMethod]
    public async Task RegisterDefaultQueryProcessor_Resolves_And_Processes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterQueryHandler<TestQuery, int, TestQueryHandler>();
        services.RegisterDefaultQueryProcessor();

        await using var provider = Build(services);
        var qp = provider.GetRequiredService<IQueryProcessor>();
        Assert.IsInstanceOfType(qp, typeof(IocQueryProcessor));

        var result = await qp.ProcessQuery<TestQuery, int>(new TestQuery());
        Assert.AreEqual(1, result);
        var probe = provider.GetRequiredService<Probe>();
        Assert.AreEqual(1, probe.InnerQueryCalls);
    }

    [TestMethod]
    public void RegisterCommandHandler_Respects_Lifetimes()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>(ServiceLifetime.Singleton);
        using (var provider = Build(services))
        {
            var a = provider.GetRequiredService<ICommandHandler<TestCommand>>();
            var b = provider.GetRequiredService<ICommandHandler<TestCommand>>();
            Assert.AreSame(a, b);
        }

        services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>(ServiceLifetime.Scoped);
        using (var provider = Build(services))
        {
            using var s1 = provider.CreateScope();
            var a1 = s1.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
            var a2 = s1.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
            Assert.AreSame(a1, a2);

            using var s2 = provider.CreateScope();
            var b1 = s2.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
            Assert.AreNotSame(a1, b1);
        }

        services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>();
        using (var provider = Build(services))
        {
            var a = provider.GetRequiredService<ICommandHandler<TestCommand>>();
            var b = provider.GetRequiredService<ICommandHandler<TestCommand>>();
            Assert.AreNotSame(a, b);
        }
    }

    [TestMethod]
    public void RegisterModuleHandlers_Registers_From_Test_Assembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterModuleCommandHandlers<TestModule>();
        services.RegisterModuleQueryHandlers<TestModule>();

        using var provider = Build(services);
        var cmdHandler = provider.GetService<ICommandHandler<TestCommand>>();
        var qryHandler = provider.GetService<IQueryHandler<TestQuery, int>>();
        Assert.IsNotNull(cmdHandler);
        Assert.IsNotNull(qryHandler);
    }

    [TestMethod]
    public async Task AddCqrs_With_Single_Decorators_Registers_Infrastructure_And_Decorates_Handlers()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>();
        services.RegisterQueryHandler<TestQuery, int, TestQueryHandler>();

        services.AddCqrs(
            ServiceLifetime.Scoped,
            typeof(CommandLoggingDecorator<>),
            typeof(QueryLoggingDecorator<,>));

        await using var provider = Build(services);

        // Infrastructure
        var bus = provider.GetRequiredService<ICommandBus>();
        var qp = provider.GetRequiredService<IQueryProcessor>();
        Assert.IsInstanceOfType(bus, typeof(IocCommandBus));
        Assert.IsInstanceOfType(qp, typeof(IocQueryProcessor));

        // Decorated handlers
        var cmdHandler = provider.GetRequiredService<ICommandHandler<TestCommand>>();
        var qryHandler = provider.GetRequiredService<IQueryHandler<TestQuery, int>>();
        Assert.IsInstanceOfType(cmdHandler, typeof(CommandLoggingDecorator<TestCommand>));
        Assert.IsInstanceOfType(qryHandler, typeof(QueryLoggingDecorator<TestQuery, int>));

        var probe = provider.GetRequiredService<Probe>();

        await bus.Send(new TestCommand());
        var result = await qp.ProcessQuery<TestQuery, int>(new TestQuery());

        Assert.AreEqual(1, result);
        Assert.AreEqual(1, probe.CommandCalls);
        Assert.AreEqual(1, probe.InnerCommandCalls);
        Assert.AreEqual(1, probe.QueryCalls);
        Assert.AreEqual(1, probe.InnerQueryCalls);
    }

    [TestMethod]
    public void AddCqrs_With_Single_Decorators_Does_Not_Fail_When_No_Handlers_Registered()
    {
        var services = new ServiceCollection();

        services.AddCqrs(
            ServiceLifetime.Scoped,
            typeof(CommandLoggingDecorator<>),
            typeof(QueryLoggingDecorator<,>));

        using var provider = Build(services);
        var bus = provider.GetRequiredService<ICommandBus>();
        var qp = provider.GetRequiredService<IQueryProcessor>();

        Assert.IsInstanceOfType(bus, typeof(IocCommandBus));
        Assert.IsInstanceOfType(qp, typeof(IocQueryProcessor));
    }

    [TestMethod]
    public async Task AddCqrs_With_Multiple_Decorators_Applies_Them_In_Parameter_Order()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Probe>();
        services.AddSingleton<DecoratorProbe>();
        services.RegisterCommandHandler<TestCommand, TestCommandHandler>();
        services.RegisterQueryHandler<TestQuery, int, TestQueryHandler>();

        services.AddCqrs(
            ServiceLifetime.Scoped,
            new[] { typeof(InnerCommandDecorator<>), typeof(OuterCommandDecorator<>) },
            new[] { typeof(InnerQueryDecorator<,>), typeof(OuterQueryDecorator<,>) });

        await using var provider = Build(services);

        var cmdHandler = provider.GetRequiredService<ICommandHandler<TestCommand>>();
        var qryHandler = provider.GetRequiredService<IQueryHandler<TestQuery, int>>();

        // The outermost decorator should be the last one in the parameter order
        Assert.IsInstanceOfType(cmdHandler, typeof(OuterCommandDecorator<TestCommand>));
        Assert.IsInstanceOfType(qryHandler, typeof(OuterQueryDecorator<TestQuery, int>));

        var decoratorProbe = provider.GetRequiredService<DecoratorProbe>();

        await cmdHandler.Handle(new TestCommand());
        var result = await qryHandler.Read(new TestQuery());

        Assert.AreEqual(1, result);

        // For commands, the call ordering should be Outer -> Inner
        CollectionAssert.AreEqual(
            new[] { "OuterCommand", "InnerCommand" },
            decoratorProbe.CommandDecoratorCallOrder);

        // For queries, the call ordering should be Outer -> Inner
        CollectionAssert.AreEqual(
            new[] { "OuterQuery", "InnerQuery" },
            decoratorProbe.QueryDecoratorCallOrder);
    }

    // Test types
    private sealed record TestCommand : ICommand;

    private sealed record TestQuery : IQuery;

    private sealed class Probe
    {
        public int CommandCalls;
        public int InnerCommandCalls;
        public int InnerQueryCalls;
        public int QueryCalls;
    }

    private sealed class DecoratorProbe
    {
        public List<string> CommandDecoratorCallOrder { get; } = new();
        public List<string> QueryDecoratorCallOrder { get; } = new();
    }

    private sealed class TestCommandHandler(Probe probe) : ICommandHandler<TestCommand>
    {
        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref probe.InnerCommandCalls);
            return Task.CompletedTask;
        }
    }

    private sealed class TestQueryHandler(Probe probe) : IQueryHandler<TestQuery, int>
    {
        public Task<int> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref probe.InnerQueryCalls);
            return Task.FromResult(1);
        }
    }

    // Valid decorators
    private sealed class CommandLoggingDecorator<TCommand>(ICommandHandler<TCommand> inner, Probe probe)
        : ICommandDecorator<TCommand> where TCommand : ICommand
    {
        public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref probe.CommandCalls);
            await inner.Handle(command, cancellationToken);
        }
    }

    private sealed class QueryLoggingDecorator<TQuery, TResult>(IQueryHandler<TQuery, TResult> inner, Probe probe)
        : IQueryDecorator<TQuery, TResult> where TQuery : IQuery
    {
        public async Task<TResult> Read(TQuery query, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref probe.QueryCalls);
            return await inner.Read(query, cancellationToken);
        }
    }

    private sealed class InnerCommandDecorator<TCommand>(ICommandHandler<TCommand> inner, DecoratorProbe probe)
        : ICommandDecorator<TCommand> where TCommand : ICommand
    {
        public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
        {
            probe.CommandDecoratorCallOrder.Add("InnerCommand");
            await inner.Handle(command, cancellationToken);
        }
    }

    private sealed class OuterCommandDecorator<TCommand>(ICommandHandler<TCommand> inner, DecoratorProbe probe)
        : ICommandDecorator<TCommand> where TCommand : ICommand
    {
        public async Task Handle(TCommand command, CancellationToken cancellationToken = default)
        {
            probe.CommandDecoratorCallOrder.Add("OuterCommand");
            await inner.Handle(command, cancellationToken);
        }
    }

    private sealed class InnerQueryDecorator<TQuery, TResult>(
        IQueryHandler<TQuery, TResult> inner,
        DecoratorProbe probe)
        : IQueryDecorator<TQuery, TResult> where TQuery : IQuery
    {
        public async Task<TResult> Read(TQuery query, CancellationToken cancellationToken = default)
        {
            probe.QueryDecoratorCallOrder.Add("InnerQuery");
            return await inner.Read(query, cancellationToken);
        }
    }

    private sealed class OuterQueryDecorator<TQuery, TResult>(
        IQueryHandler<TQuery, TResult> inner,
        DecoratorProbe probe)
        : IQueryDecorator<TQuery, TResult> where TQuery : IQuery
    {
        public async Task<TResult> Read(TQuery query, CancellationToken cancellationToken = default)
        {
            probe.QueryDecoratorCallOrder.Add("OuterQuery");
            return await inner.Read(query, cancellationToken);
        }
    }

    // Invalid decorators (do not implement ICommandDecorator/IQueryDecorator)
    private sealed class InvalidCommandDecorator<TCommand> : ICommandHandler<TCommand> where TCommand : ICommand
    {
        public Task Handle(TCommand command, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class InvalidQueryDecorator<TQuery, TResult> : IQueryHandler<TQuery, TResult> where TQuery : IQuery
    {
        public Task<TResult> Read(TQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(default(TResult)!);
        }
    }

    // Module for assembly scanning tests
    private sealed class TestModule : IModule
    {
        public IConfiguration? Configuration { private get; set; }

        public void Initialize(IServiceCollection services)
        {
            // Explicit registrations not required; RegisterModule* scans assembly of this type.
        }
    }

    // Additional handlers in the same assembly for RegisterModule* to discover
    private sealed class ModuleCommandHandler : ICommandHandler<TestCommand>
    {
        public Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ModuleQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> Read(TestQuery query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(123);
        }
    }
}