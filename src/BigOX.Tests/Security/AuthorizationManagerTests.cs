using System.Security;
using BigOX.Security;
using Microsoft.Extensions.DependencyInjection;

namespace BigOX.Tests.Security;

[TestClass]
public sealed class AuthorizationManagerTests
{
    private static ServiceProvider BuildProvider(
        Action<AuthorizationOptions>? configure = null,
        Action<IServiceCollection>? registerRules = null)
    {
        var services = new ServiceCollection();
        services.AddAuthorizationSecurity(ServiceLifetime.Scoped, configure);
        registerRules?.Invoke(services);
        return services.BuildServiceProvider();
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestArgs(string Name);

    private sealed class PassingRule : IAuthorizationRule<TestArgs>
    {
        public ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs, CancellationToken cancellationToken = default)
        {
            return new ValueTask<AuthorizationResult>(AuthorizationResult.Success());
        }
    }

    private sealed class FailingRule : IAuthorizationRule<TestArgs>
    {
        public ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs, CancellationToken cancellationToken = default)
        {
            return new ValueTask<AuthorizationResult>(AuthorizationResult.Failure("Denied by FailingRule"));
        }
    }

    [TestMethod]
    public async Task Evaluate_NoRules_Allow_Succeeds_WithHasRulesFalse()
    {
        await using var provider = BuildProvider(o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Allow);
        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("a"));

        Assert.IsTrue(result.IsSuccessful);
        Assert.IsFalse(result.HasRules);
        Assert.IsEmpty(result.Failures);
    }

    [TestMethod]
    public async Task Evaluate_NoRules_Deny_Fails_WithNoRulesFailure()
    {
        await using var provider = BuildProvider(o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Deny);
        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("a"));

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsFalse(result.HasRules);
        Assert.HasCount(1, result.Failures);
        Assert.AreEqual("NoRulesConfigured", result.Failures[0].Code);
    }

    [TestMethod]
    public async Task Evaluate_NoRules_Error_ThrowsInvalidOperationException()
    {
        await using var provider = BuildProvider(o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error);
        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        try
        {
            _ = await auth.EvaluateAsync(new TestArgs("a"));
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // expected
        }
    }

    [TestMethod]
    public async Task Evaluate_AllRulesPass_Succeeds_WithHasRulesTrue()
    {
        await using var provider = BuildProvider(
            o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error,
            services =>
            {
                services.AddScoped<IAuthorizationRule<TestArgs>, PassingRule>();
                services.AddScoped<IAuthorizationRule<TestArgs>, PassingRule>();
            });

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("ok"));

        Assert.IsTrue(result.IsSuccessful);
        Assert.IsTrue(result.HasRules);
        Assert.IsEmpty(result.Failures);
    }

    [TestMethod]
    public async Task Evaluate_SomeRulesFail_ReturnsFailures()
    {
        await using var provider = BuildProvider(
            o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error,
            services =>
            {
                services.AddScoped<IAuthorizationRule<TestArgs>, PassingRule>();
                services.AddScoped<IAuthorizationRule<TestArgs>, FailingRule>();
                services.AddScoped<IAuthorizationRule<TestArgs>, FailingRule>();
            });

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("x"));

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsTrue(result.HasRules);
        Assert.HasCount(2, result.Failures);
        Assert.IsTrue(result.Failures.All(f => f.RuleType == typeof(FailingRule)));
    }

    [TestMethod]
    public async Task AuthorizeAsync_Failure_ThrowsSecurityException()
    {
        await using var provider = BuildProvider(
            o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error,
            services => services.AddScoped<IAuthorizationRule<TestArgs>, FailingRule>());

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        try
        {
            await auth.AuthorizeAsync(new TestArgs("x"));
            Assert.Fail("Expected SecurityException");
        }
        catch (SecurityException)
        {
            // expected
        }
    }

    [TestMethod]
    public async Task Evaluate_CanceledToken_ThrowsOperationCanceled()
    {
        await using var provider = BuildProvider(o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Allow);
        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        try
        {
            _ = await auth.EvaluateAsync(new TestArgs("y"), cts.Token);
            Assert.Fail("Expected OperationCanceledException");
        }
        catch (OperationCanceledException)
        {
            // expected
        }
    }
}
