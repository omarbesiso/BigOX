namespace BigOX.Security;

/// <summary>
///     Represents configuration options for the <see cref="IAuthorizationManager" />.
/// </summary>
public sealed class AuthorizationOptions
{
    /// <summary>
    ///     Gets or sets the behavior to apply when no authorization rules are registered
    ///     for a given authorization argument type.
    ///     The default value is <see cref="AuthorizationNoRulesBehavior.Error" />.
    /// </summary>
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public AuthorizationNoRulesBehavior NoRulesBehavior { get; set; } =
        AuthorizationNoRulesBehavior.Error;

    /// <summary>
    ///     Gets or sets a value indicating whether authorization rules are evaluated concurrently.
    ///     The default value is <see langword="false" />, which preserves sequential evaluation in registration order.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When <see langword="false" /> (the default), rules are evaluated one at a time in the order they were
    ///         registered, short-circuiting nothing but observing each rule sequentially.
    ///     </para>
    ///     <para>
    ///         When <see langword="true" /> and more than one rule is registered, all rules are started concurrently and
    ///         awaited together. Failures are still collected in rule registration order, so the resulting failure
    ///         ordering is identical to sequential evaluation. Enable this only when the registered rules are safe to run
    ///         in parallel (for example, they do not share non-thread-safe scoped state). If a rule throws, the first
    ///         exception surfaces, matching the sequential first-throw behavior.
    ///     </para>
    /// </remarks>
    public bool EvaluateRulesInParallel { get; set; }
}