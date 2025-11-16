namespace BigOX.Security;

/// <summary>
///     Specifies how <see cref="IAuthorizationManager" /> should behave when
///     no <see cref="IAuthorizationRule{TAuthorizationArgs}" /> instances
///     are registered for a given authorization argument type.
/// </summary>
public enum AuthorizationNoRulesBehavior
{
    /// <summary>
    ///     Treat the absence of authorization rules as a successful authorization.
    ///     This mirrors the behavior of some legacy systems but is typically not recommended
    ///     for security-sensitive operations.
    /// </summary>
    Allow = 0,

    /// <summary>
    ///     Treat the absence of authorization rules as a failed authorization.
    ///     A single <see cref="AuthorizationFailure" /> will be produced, indicating
    ///     that no rules were configured.
    /// </summary>
    Deny = 1,

    /// <summary>
    ///     Treat the absence of authorization rules as a configuration error and throw
    ///     an <see cref="InvalidOperationException" /> from the evaluation call.
    /// </summary>
    Error = 2
}