using BigOX.DependencyInjection;
using BigOX.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Tests.Domain;

// Public top-level event and handlers so Scrutor (PublicOnly default) can discover them during assembly scan
public class TestDomainEvent : IDomainEvent;

public class TestDomainEventHandlerA : IDomainEventHandler<TestDomainEvent>
{
    public int CallCount { get; private set; }

    public Task Handle(TestDomainEvent @event, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}

public class TestDomainEventHandlerB : IDomainEventHandler<TestDomainEvent>
{
    public Task Handle(TestDomainEvent @event, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

[TestClass]
public class DomainServiceCollectionExtensionsTests
{
    [TestMethod]
    public void RegisterDomainEventHandler_Transient_ReturnsDifferentInstances()
    {
        var services = new ServiceCollection();
        services.RegisterDomainEventHandler<TestDomainEvent, TestDomainEventHandlerA>();
        using var provider = services.BuildServiceProvider();

        var h1 = provider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();
        var h2 = provider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();

        Assert.IsNotNull(h1);
        Assert.IsNotNull(h2);
        Assert.AreNotSame(h1, h2, "Transient lifetime should create different instances");
    }

    [TestMethod]
    public void RegisterDomainEventHandler_Scoped_ReturnsSameInScopeDifferentAcrossScopes()
    {
        var services = new ServiceCollection();
        services.RegisterDomainEventHandler<TestDomainEvent, TestDomainEventHandlerA>(ServiceLifetime.Scoped);
        using var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        var s1H1 = scope1.ServiceProvider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();
        var s1H2 = scope1.ServiceProvider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();
        Assert.AreSame(s1H1, s1H2, "Scoped lifetime should return same instance within the same scope");

        using var scope2 = provider.CreateScope();
        var s2H = scope2.ServiceProvider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();
        Assert.AreNotSame(s1H1, s2H, "Scoped lifetime should return different instances across scopes");
    }

    [TestMethod]
    public void RegisterDomainEventHandler_Singleton_ReturnsSameInstance()
    {
        var services = new ServiceCollection();
        services.RegisterDomainEventHandler<TestDomainEvent, TestDomainEventHandlerA>(ServiceLifetime.Singleton);
        using var provider = services.BuildServiceProvider();

        var h1 = provider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();
        var h2 = provider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();
        Assert.AreSame(h1, h2, "Singleton lifetime should return the same instance");
    }

    [TestMethod]
    public void RegisterDefaultDomainEventBus_RegistersIocImplementation_WithSingletonLifetime()
    {
        var services = new ServiceCollection();
        services.RegisterDefaultDomainEventBus();
        using var provider = services.BuildServiceProvider();

        var bus1 = provider.GetRequiredService<IDomainEventBus>();
        var bus2 = provider.GetRequiredService<IDomainEventBus>();

        Assert.IsNotNull(bus1);
        Assert.AreEqual("IocDomainEventBus", bus1.GetType().Name,
            "Should register default IoC-based bus implementation");
        Assert.AreSame(bus1, bus2, "Expected singleton lifetime for bus");
    }

    [TestMethod]
    public void RegisterDefaultDomainEventBus_ScopedLifetime_BehavesAsScoped()
    {
        var services = new ServiceCollection();
        services.RegisterDefaultDomainEventBus(ServiceLifetime.Scoped);
        using var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        var s1Bus1 = scope1.ServiceProvider.GetRequiredService<IDomainEventBus>();
        var s1Bus2 = scope1.ServiceProvider.GetRequiredService<IDomainEventBus>();
        Assert.AreSame(s1Bus1, s1Bus2);

        using var scope2 = provider.CreateScope();
        var s2Bus = scope2.ServiceProvider.GetRequiredService<IDomainEventBus>();
        Assert.AreNotSame(s1Bus1, s2Bus);
    }

    [TestMethod]
    public async Task RegisterDefaultDomainEventBus_WiresToRegisteredHandlers()
    {
        var services = new ServiceCollection();
        services
            .RegisterDefaultDomainEventBus()
            .RegisterDomainEventHandler<TestDomainEvent, TestDomainEventHandlerA>(ServiceLifetime.Singleton);

        using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IDomainEventBus>();
        var handler = (TestDomainEventHandlerA)provider.GetRequiredService<IDomainEventHandler<TestDomainEvent>>();

        Assert.AreEqual(0, handler.CallCount);
        await bus.Publish(new TestDomainEvent());
        Assert.AreEqual(1, handler.CallCount);
    }

    [TestMethod]
    public void RegisterModuleDomainEventHandlers_RegistersAllHandlersFromModuleAssembly()
    {
        var services = new ServiceCollection();
        services.RegisterModuleDomainEventHandlers<TestModule>();
        using var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IDomainEventHandler<TestDomainEvent>>().ToList();

        Assert.HasCount(2, handlers, "Should auto-register all handlers from the module assembly");
        var names = handlers.Select(h => h.GetType().Name).ToList();
        CollectionAssert.Contains(names, nameof(TestDomainEventHandlerA));
        CollectionAssert.Contains(names, nameof(TestDomainEventHandlerB));

        // Scoped behavior: resolving again within the same scope should return the same instances regardless of enumeration order
        var handlersAgain = scope.ServiceProvider.GetServices<IDomainEventHandler<TestDomainEvent>>().ToList();
        Assert.HasCount(handlers.Count, handlersAgain);

        var map = handlers.ToDictionary(h => h.GetType(), h => h);
        foreach (var h in handlersAgain)
        {
            Assert.IsTrue(map.TryGetValue(h.GetType(), out var original));
            Assert.AreSame(original, h);
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class TestModule : IModule
    {
        public IConfiguration? Configuration { private get; set; }

        public void Initialize(IServiceCollection serviceCollection)
        {
            // No explicit registrations needed for these tests
        }
    }
}