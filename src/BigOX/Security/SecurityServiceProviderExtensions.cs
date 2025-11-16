using BigOX.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Security;

/// <summary>
///     Provides extension methods for retrieving authorization rules from an
///     <see cref="IServiceProvider" />.
/// </summary>
public static class SecurityServiceProviderExtensions
{
    /// <summary>
    ///     Retrieves all registered implementations of
    ///     <see cref="IAuthorizationRule{TAuthorizationArgs}" /> from the specified
    ///     <paramref name="serviceProvider" />.
    /// </summary>
    /// <typeparam name="TAuthorizationArgs">
    ///     The type of authorization arguments for which the authorization rules apply.
    /// </typeparam>
    /// <param name="serviceProvider">
    ///     The <see cref="IServiceProvider" /> from which the authorization rules will be retrieved.
    /// </param>
    /// <returns>
    ///     An <see cref="IEnumerable{T}" /> of all registered <see cref="IAuthorizationRule{TAuthorizationArgs}" />
    ///     implementations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="serviceProvider" /> is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This method is primarily intended for internal use and advanced scenarios.
    ///     Most consumers should rely on <see cref="IAuthorizationManager" /> instead of
    ///     resolving rules directly.
    /// </remarks>
    public static IEnumerable<IAuthorizationRule<TAuthorizationArgs>> GetAuthorizationRules<TAuthorizationArgs>(
        this IServiceProvider serviceProvider)
    {
        Guard.NotNull(serviceProvider);
        return serviceProvider.GetServices<IAuthorizationRule<TAuthorizationArgs>>();
    }
}