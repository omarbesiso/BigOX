using BigOX.DependencyInjection;
using BigOX.Tests.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

namespace BigOX.Tests.DependencyInjection;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    private static IConfiguration BuildConfig(string key = "x", string value = "42")
    {
        return new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { key, value } })
            .Build();
    }

    private static ServiceProvider BuildProvider(ServiceCollection services)
    {
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void AddModule_Initializes_Module_And_Sets_Configuration()
    {
        var services = new ServiceCollection();
        var config = BuildConfig();

        services.AddModule<TestModuleWithConfig>(config);
        using var provider = BuildProvider(services);

        var marker = provider.GetRequiredService<IModuleMarker>();
        Assert.IsNotNull(marker);

        var probes = provider.GetServices<IConfigProbe>().ToList();
        Assert.HasCount(1, probes);
        Assert.AreEqual("42", probes[0].Value);
    }

    [TestMethod]
    public void AddAllModules_Loads_And_Initializes_All_Modules()
    {
        var services = new ServiceCollection();
        var config = BuildConfig("x", "99");

        services.AddAllModules(config);
        using var provider = BuildProvider(services);

        var markers = provider.GetServices<IModuleMarker>().ToList();
        // Expect both markers from modules defined in this test assembly
        Assert.IsTrue(markers.Any(m => m is ModuleAMarker));
        Assert.IsTrue(markers.Any(m => m is ModuleBMarker));

        var probes = provider.GetServices<IConfigProbe>().ToList();
        // Both modules register a probe using the provided configuration
        Assert.IsGreaterThanOrEqualTo(2, probes.Count);
        foreach (var p in probes)
        {
            Assert.AreEqual("99", p.Value);
        }
    }

    [TestMethod]
    public void AddTypesFromAssembly_Registers_Assignable_Types_As_Transient_By_Default()
    {
        var services = new ServiceCollection();

        services.AddTypesFromAssembly<FooA, IFoo>();
        using var provider = BuildProvider(services);

        var foos = provider.GetServices<IFoo>().ToList();
        Assert.IsTrue(foos.Any(f => f is FooA));
        Assert.IsTrue(foos.Any(f => f is FooB));

        // Transient should produce different instances
        var f1 = provider.GetRequiredService<IFoo>();
        var f2 = provider.GetRequiredService<IFoo>();
        Assert.AreNotSame(f1, f2);
    }

    [TestMethod]
    public void AddTypesFromAssembly_WithScoped_Lifetime_Resolves_Per_Scope()
    {
        var services = new ServiceCollection();

        services.AddTypesFromAssembly<ScopedSvc, IScopedSvc>(ServiceLifetime.Scoped);
        using var provider = BuildProvider(services);

        using var s1 = provider.CreateScope();
        var a1 = s1.ServiceProvider.GetRequiredService<IScopedSvc>();
        var a2 = s1.ServiceProvider.GetRequiredService<IScopedSvc>();
        Assert.AreSame(a1, a2);

        using var s2 = provider.CreateScope();
        var b1 = s2.ServiceProvider.GetRequiredService<IScopedSvc>();
        Assert.AreNotSame(a1, b1);
    }

    [TestMethod]
    public void AddTypesFromAssembly_WithSingleton_Lifetime_Resolves_Same_Instance()
    {
        var services = new ServiceCollection();

        services.AddTypesFromAssembly<SingletonSvc, ISingletonSvc>(ServiceLifetime.Singleton);
        using var provider = BuildProvider(services);

        var s1 = provider.GetRequiredService<ISingletonSvc>();
        var s2 = provider.GetRequiredService<ISingletonSvc>();
        Assert.AreSame(s1, s2);
    }

    [TestMethod]
    public void AddModule_Throws_On_Null_ServiceCollection()
    {
        ServiceCollection? services = null;
        var config = BuildConfig();
        var ex = TestUtils.Expect<ArgumentNullException>(() => services!.AddModule<TestModuleWithConfig>(config));
        Assert.Contains(nameof(services), ex.ParamName!);
    }

    // Marker interfaces and services for testing AddTypesFromAssembly
    public interface IFoo;

    public class FooA : IFoo;

    public class FooB : IFoo;

    public interface IBar;

    public class BarImpl : IBar;

    public interface IScopedSvc;

    public class ScopedSvc : IScopedSvc;

    public interface ISingletonSvc;

    public class SingletonSvc : ISingletonSvc;

    // Probes for module tests
    public interface IModuleMarker;

    public class ModuleAMarker : IModuleMarker;

    public class ModuleBMarker : IModuleMarker;

    public interface IConfigProbe
    {
        string? Value { get; }
    }

    public class ConfigProbe(string? value) : IConfigProbe
    {
        public string? Value { get; } = value;
    }

    // Test modules defined in the test assembly to be discovered by AddAllModules
    public class TestModuleWithConfig : IModule
    {
        private IConfiguration? _config;

        public IConfiguration? Configuration
        {
            set => _config = value;
        }

        public void Initialize(IServiceCollection services)
        {
            services.AddSingleton<IModuleMarker, ModuleAMarker>();
            if (_config != null)
            {
                services.AddSingleton<IConfigProbe>(new ConfigProbe(_config["x"]));
            }
        }
    }

    public class TestModuleB : IModule
    {
        private IConfiguration? _config;

        public IConfiguration? Configuration
        {
            set => _config = value;
        }

        public void Initialize(IServiceCollection services)
        {
            services.AddSingleton<IModuleMarker, ModuleBMarker>();
            if (_config != null)
            {
                services.AddSingleton<IConfigProbe>(new ConfigProbe(_config["x"]));
            }
        }
    }
}