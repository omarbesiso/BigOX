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
}