using System.Reflection;
using System.Runtime.Loader;
using BigOX.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.DependencyInjection;

/// <summary>
///     Class providing extensions to the <see cref="IServiceCollection" /> to allow for the registration of different
///     types of handlers for different message types.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static IEnumerable<Assembly> LoadAssembliesFromPath(string path)
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
        var loadedPaths = new HashSet<string>(
            loadedAssemblies
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .Select(a => a.Location),
            StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.EnumerateFiles(path, "*.dll"))
        {
            if (loadedPaths.Contains(file))
            {
                continue;
            }

            try
            {
                var asmName = AssemblyName.GetAssemblyName(file);
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(asmName);
                loadedAssemblies.Add(asm);
            }
            catch
            {
                // Ignore files that are not valid .NET assemblies or cannot be loaded
            }
        }

        return loadedAssemblies;
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null).Cast<Type>();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    ///     Provides extension methods for IServiceCollection to load modules and register services.
    /// </summary>
    /// <param name="serviceCollection">The service collection to contain the registrations.</param>
    extension(IServiceCollection serviceCollection)
    {
        /// <summary>
        ///     Loads a module by applying all the specified registrations and configurations to the supplied service collection.
        /// </summary>
        /// <typeparam name="TModule">The type of the module.</typeparam>
        /// <param name="configuration">The optional configuration instance to be used for registrations.</param>
        /// <returns>A reference to this service collection instance after the operation has completed.</returns>
        public IServiceCollection AddModule<TModule>(IConfiguration? configuration = null)
            where TModule : IModule, new()
        {
            Guard.NotNull(serviceCollection, "services");

            var module = new TModule();
            if (configuration != null)
            {
                module.Configuration = configuration;
            }

            module.Initialize(serviceCollection);
            return serviceCollection;
        }

        /// <summary>
        ///     Loads all <see cref="IModule" /> implementations found in loaded assemblies and initializes them.
        /// </summary>
        /// <param name="configuration">The optional configuration instance to be used for registrations.</param>
        /// <returns>A reference to this service collection instance after the operation has completed.</returns>
        public IServiceCollection AddAllModules(IConfiguration? configuration = null)
        {
            Guard.NotNull(serviceCollection, "services");

            var interfaceType = typeof(IModule);
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var loadedAssemblies = LoadAssembliesFromPath(path);

            var moduleTypes = loadedAssemblies
                .SelectMany(GetLoadableTypes)
                .Where(type =>
                    type is { IsInterface: false, IsAbstract: false } && interfaceType.IsAssignableFrom(type))
                .ToList();

            foreach (var moduleType in moduleTypes)
            {
                try
                {
                    var module = (IModule)Activator.CreateInstance(moduleType)!;
                    if (configuration != null)
                    {
                        module.Configuration = configuration;
                    }

                    module.Initialize(serviceCollection);
                }
                catch
                {
                    // Ignore types that cannot be instantiated
                }
            }

            return serviceCollection;
        }

        /// <summary>
        ///     Adds services from an assembly containing a specified type to the <see cref="IServiceCollection" /> with the
        ///     specified <see cref="ServiceLifetime" />.
        /// </summary>
        /// <typeparam name="TAssemblyType">A type contained within the target assembly.</typeparam>
        /// <typeparam name="TBase">A base type or interface that the services should be assignable to.</typeparam>
        /// <param name="serviceLifetime">
        ///     The desired <see cref="ServiceLifetime" /> for the added services. Defaults to
        ///     <see cref="ServiceLifetime.Transient" />.
        /// </param>
        /// <returns>The updated <see cref="IServiceCollection" /> with the added services.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when an unsupported <see cref="ServiceLifetime" /> value is passed.
        /// </exception>
        public IServiceCollection AddTypesFromAssembly<TAssemblyType, TBase>(
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            Guard.NotNull(serviceCollection, "services");

            switch (serviceLifetime)
            {
                case ServiceLifetime.Scoped:
                    serviceCollection.Scan(scan =>
                        scan.FromAssemblyOf<TAssemblyType>()
                            .AddClasses(classes => classes.AssignableTo<TBase>())
                            .AsImplementedInterfaces()
                            .WithScopedLifetime());
                    break;
                case ServiceLifetime.Transient:
                    serviceCollection.Scan(scan =>
                        scan.FromAssemblyOf<TAssemblyType>()
                            .AddClasses(classes => classes.AssignableTo<TBase>())
                            .AsImplementedInterfaces()
                            .WithTransientLifetime());
                    break;
                case ServiceLifetime.Singleton:
                    serviceCollection.Scan(scan =>
                        scan.FromAssemblyOf<TAssemblyType>()
                            .AddClasses(classes => classes.AssignableTo<TBase>())
                            .AsImplementedInterfaces()
                            .WithSingletonLifetime());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(serviceLifetime), serviceLifetime, null);
            }

            return serviceCollection;
        }
    }
}