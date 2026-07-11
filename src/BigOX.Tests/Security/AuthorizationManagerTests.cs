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

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task Evaluate_FailureOrder_IsRegistrationOrder_RegardlessOfParallelism(bool parallel)
    {
        await using var provider = BuildProvider(
            o =>
            {
                o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error;
                o.EvaluateRulesInParallel = parallel;
            },
            services =>
            {
                services.AddScoped<IAuthorizationRule<TestArgs>, SlowFailingRuleA>();
                services.AddScoped<IAuthorizationRule<TestArgs>, FastFailingRuleB>();
            });

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("x"));

        Assert.IsFalse(result.IsSuccessful);
        Assert.HasCount(2, result.Failures);

        // Registration order is preserved even though A is deliberately slower than B.
        Assert.AreEqual(typeof(SlowFailingRuleA), result.Failures[0].RuleType);
        Assert.AreEqual(typeof(FastFailingRuleB), result.Failures[1].RuleType);
        Assert.AreEqual("CODE_A", result.Failures[0].Code);
        Assert.AreEqual("CODE_B", result.Failures[1].Code);
    }

    [TestMethod]
    public async Task Evaluate_ParallelMode_RunsRulesConcurrently()
    {
        var rendezvous = new Rendezvous();
        await using var provider = BuildProvider(
            o =>
            {
                o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error;
                o.EvaluateRulesInParallel = true;
            },
            services =>
            {
                services.AddSingleton(rendezvous);
                services.AddScoped<IAuthorizationRule<TestArgs>, RendezvousRuleA>();
                services.AddScoped<IAuthorizationRule<TestArgs>, RendezvousRuleB>();
            });

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        // Each rule signals its own gate then waits for the other's. This only completes if the two rules
        // run concurrently; sequential evaluation would deadlock and each WaitAsync(5s) would time out.
        var result = await auth.EvaluateAsync(new TestArgs("x"));

        Assert.IsTrue(result.IsSuccessful);
    }

    [TestMethod]
    public async Task Evaluate_FailureWithCode_FlowsCodeIntoAuthorizationFailure()
    {
        await using var provider = BuildProvider(
            o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error,
            services => services.AddScoped<IAuthorizationRule<TestArgs>, CodedFailingRule>());

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("x"));

        Assert.IsFalse(result.IsSuccessful);
        Assert.HasCount(1, result.Failures);
        Assert.AreEqual("POLICY_X", result.Failures[0].Code);
        Assert.AreEqual("Denied", result.Failures[0].Message);
    }

    [TestMethod]
    public async Task Evaluate_FailureWithoutCode_KeepsNullCode()
    {
        await using var provider = BuildProvider(
            o => o.NoRulesBehavior = AuthorizationNoRulesBehavior.Error,
            services => services.AddScoped<IAuthorizationRule<TestArgs>, FailingRule>());

        using var scope = provider.CreateScope();
        var auth = scope.ServiceProvider.GetRequiredService<IAuthorizationManager>();

        var result = await auth.EvaluateAsync(new TestArgs("x"));

        Assert.IsFalse(result.IsSuccessful);
        Assert.IsNull(result.Failures[0].Code);
    }

    [TestMethod]
    public void AuthorizationResult_FailureWithCode_SetsCode()
    {
        var r = AuthorizationResult.Failure("denied", "CODE1");
        Assert.IsFalse(r.Successful);
        Assert.AreEqual("CODE1", r.Code);
        Assert.AreEqual("denied", r.Message);
    }

    [TestMethod]
    public void AuthorizationResult_FailureWithoutCode_HasNullCode()
    {
        var r = AuthorizationResult.Failure("denied");
        Assert.IsFalse(r.Successful);
        Assert.IsNull(r.Code);
    }

    [TestMethod]
    public void AuthorizationResult_Success_HasNullCode()
    {
        var r = AuthorizationResult.Success();
        Assert.IsTrue(r.Successful);
        Assert.IsNull(r.Code);
    }

    // ReSharper disable once NotAccessedPositionalProperty.Local
    private sealed record TestArgs(string Name);

    private sealed class PassingRule : IAuthorizationRule<TestArgs>
    {
        public ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default) => new(AuthorizationResult.Success());
    }

    private sealed class FailingRule : IAuthorizationRule<TestArgs>
    {
        public ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default) => new(AuthorizationResult.Failure("Denied by FailingRule"));
    }

    private sealed class CodedFailingRule : IAuthorizationRule<TestArgs>
    {
        public ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default) =>
            new(AuthorizationResult.Failure("Denied", "POLICY_X"));
    }

    private sealed class SlowFailingRuleA : IAuthorizationRule<TestArgs>
    {
        public async ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(60, cancellationToken);
            return AuthorizationResult.Failure("A failed", "CODE_A");
        }
    }

    private sealed class FastFailingRuleB : IAuthorizationRule<TestArgs>
    {
        public ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default) =>
            new(AuthorizationResult.Failure("B failed", "CODE_B"));
    }

    private sealed class Rendezvous
    {
        public TaskCompletionSource GateA { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource GateB { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private sealed class RendezvousRuleA(Rendezvous rendezvous) : IAuthorizationRule<TestArgs>
    {
        public async ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default)
        {
            rendezvous.GateA.TrySetResult();
            await rendezvous.GateB.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            return AuthorizationResult.Success();
        }
    }

    private sealed class RendezvousRuleB(Rendezvous rendezvous) : IAuthorizationRule<TestArgs>
    {
        public async ValueTask<AuthorizationResult> IsAuthorizedAsync(TestArgs authorizationArgs,
            CancellationToken cancellationToken = default)
        {
            rendezvous.GateB.TrySetResult();
            await rendezvous.GateA.Task.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);
            return AuthorizationResult.Success();
        }
    }
}