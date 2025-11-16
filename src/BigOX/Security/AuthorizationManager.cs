using BigOX.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BigOX.Security;

/// <summary>
///     Default implementation of <see cref="IAuthorizationManager" /> that uses
///     DI-registered <see cref="IAuthorizationRule{TAuthorizationArgs}" /> instances
///     to evaluate authorization for a given operation.
/// </summary>
internal sealed class AuthorizationManager : IAuthorizationManager
{
    private readonly ILogger<AuthorizationManager>? _logger;
    private readonly AuthorizationOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationManager" /> class.
    /// </summary>
    /// <param name="scopeFactory">
    ///     The <see cref="IServiceScopeFactory" /> used to create scopes for resolving
    ///     scoped authorization rules safely.
    /// </param>
    /// <param name="options">
    ///     The configured <see cref="AuthorizationOptions" />.
    /// </param>
    /// <param name="logger">
    ///     An optional logger for diagnostic and audit information.
    /// </param>
    public AuthorizationManager(
        IServiceScopeFactory scopeFactory,
        IOptions<AuthorizationOptions> options,
        ILogger<AuthorizationManager>? logger = null)
    {
        _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<AuthorizationEvaluationResult> EvaluateAsync<TAuthorizationArgs>(
        TAuthorizationArgs authorizationArgs,
        CancellationToken cancellationToken = default)
    {
        Guard.NotNull(authorizationArgs, "Authorization arguments cannot be null.");

        cancellationToken.ThrowIfCancellationRequested();

        using var scope = _scopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var rules = serviceProvider
            .GetAuthorizationRules<TAuthorizationArgs>()
            .ToArray();

        var argsType = typeof(TAuthorizationArgs);

        if (rules.Length == 0)
        {
            switch (_options.NoRulesBehavior)
            {
                case AuthorizationNoRulesBehavior.Allow:
                    _logger?.LogWarning(
                        "No authorization rules are registered for authorization arguments of type {AuthorizationArgsType}. " +
                        "Allowing the request based on {NoRulesBehavior}.",
                        argsType,
                        _options.NoRulesBehavior);
                    return AuthorizationEvaluationResult.Success(false);

                case AuthorizationNoRulesBehavior.Deny:
                    _logger?.LogWarning(
                        "No authorization rules are registered for authorization arguments of type {AuthorizationArgsType}. " +
                        "Denying the request based on {NoRulesBehavior}.",
                        argsType,
                        _options.NoRulesBehavior);
                    return AuthorizationEvaluationResult.NoRulesDeny(argsType);

                case AuthorizationNoRulesBehavior.Error:
                    var message =
                        $"No authorization rules are configured for arguments of type '{argsType.FullName}'.";
                    _logger?.LogError(
                        "No authorization rules are registered for authorization arguments of type {AuthorizationArgsType}. " +
                        "Throwing {ExceptionType} based on {NoRulesBehavior}.",
                        argsType,
                        typeof(InvalidOperationException).FullName,
                        _options.NoRulesBehavior);
                    throw new InvalidOperationException(message);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(_options.NoRulesBehavior),
                        _options.NoRulesBehavior,
                        "Unsupported no-rules behavior.");
            }
        }

        var failures = new List<AuthorizationFailure>(rules.Length);

        foreach (var rule in rules)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await rule
                .IsAuthorizedAsync(authorizationArgs, cancellationToken)
                .ConfigureAwait(false);

            if (!result.Successful)
            {
                var failure = AuthorizationFailure.From(result, rule.GetType());
                failures.Add(failure);
            }
        }

        if (failures.Count == 0)
        {
            return AuthorizationEvaluationResult.Success();
        }

        _logger?.LogWarning(
            "Authorization failed for arguments of type {AuthorizationArgsType}. " +
            "{FailureCount} authorization rule(s) reported failures.",
            argsType,
            failures.Count);

        return AuthorizationEvaluationResult.Failed(failures);
    }

    /// <inheritdoc />
    public async ValueTask AuthorizeAsync<TAuthorizationArgs>(
        TAuthorizationArgs authorizationArgs,
        CancellationToken cancellationToken = default)
    {
        var evaluation = await EvaluateAsync(authorizationArgs, cancellationToken)
            .ConfigureAwait(false);

        if (evaluation.IsSuccessful)
        {
            return;
        }

        throw evaluation.ToSecurityException();
    }
}