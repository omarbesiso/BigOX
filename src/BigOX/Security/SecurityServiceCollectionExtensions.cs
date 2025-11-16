using BigOX.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Security;

/// <summary>
///     Provides extension methods for registering authorization services
///     with an <see cref="IServiceCollection" />.
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the authorization manager and related configuration to the specified
    ///     <paramref name="serviceCollection" /> using the provided <paramref name="lifetime" />
    ///     and optional configuration delegate.
    /// </summary>
    /// <param name="serviceCollection">
    ///     The service collection to which authorization services will be added.
    /// </param>
    /// <param name="lifetime">
    ///     The <see cref="ServiceLifetime" /> for the <see cref="IAuthorizationManager" /> service.
    ///     The default value is <see cref="ServiceLifetime.Scoped" />.
    /// </param>
    /// <param name="configureOptions">
    ///     An optional delegate used to configure <see cref="AuthorizationOptions" />.
    /// </param>
    /// <returns>
    ///     A reference to the same <see cref="IServiceCollection" /> instance to support
    ///     fluent registration syntax.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="serviceCollection" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddAuthorizationSecurity(
        this IServiceCollection serviceCollection,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        Action<AuthorizationOptions>? configureOptions = null)
    {
        Guard.NotNull(serviceCollection);

        if (configureOptions is not null)
        {
            serviceCollection.Configure(configureOptions);
        }

        serviceCollection.Add(new ServiceDescriptor(
            typeof(IAuthorizationManager),
            typeof(AuthorizationManager),
            lifetime));

        return serviceCollection;
    }
}