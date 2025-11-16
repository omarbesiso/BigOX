// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BigOX.Security;

/// <summary>
///     Represents a normalized, structured description of an authorization failure.
/// </summary>
public readonly record struct AuthorizationFailure
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuthorizationFailure" /> struct.
    /// </summary>
    /// <param name="message">
    ///     A non-empty, non-sensitive message describing the failure.
    /// </param>
    /// <param name="code">
    ///     An optional, stable code that identifies the failure (for example, a policy or rule code).
    /// </param>
    /// <param name="ruleType">
    ///     The concrete <see cref="Type" /> of the rule that produced the failure.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="ruleType" /> is <c>null</c>.
    /// </exception>
    // ReSharper disable once MemberCanBePrivate.Global
    public AuthorizationFailure(string message, string? code, Type ruleType)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            message = "Authorization rule failed.";
        }

        Message = message;
        Code = code;
        RuleType = ruleType ?? throw new ArgumentNullException(nameof(ruleType));
    }

    /// <summary>
    ///     Gets the non-sensitive message describing the failure.
    /// </summary>
    public string Message { get; }

    /// <summary>
    ///     Gets the optional, stable code that identifies the failure.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    ///     Gets the concrete rule type that produced the failure.
    /// </summary>
    public Type RuleType { get; }

    /// <summary>
    ///     Creates an <see cref="AuthorizationFailure" /> instance from
    ///     an <see cref="AuthorizationResult" /> and a rule type.
    /// </summary>
    /// <param name="result">
    ///     The rule evaluation result.
    /// </param>
    /// <param name="ruleType">
    ///     The concrete rule type that produced the result.
    /// </param>
    /// <param name="code">
    ///     An optional code that identifies the failure.
    /// </param>
    /// <returns>
    ///     An <see cref="AuthorizationFailure" /> instance representing the failed result.
    /// </returns>
    internal static AuthorizationFailure From(
        AuthorizationResult result,
        Type ruleType,
        string? code = null)
    {
        var message = string.IsNullOrWhiteSpace(result.Message)
            ? "Authorization rule failed."
            : result.Message;

        return new AuthorizationFailure(message, code, ruleType);
    }

    /// <summary>
    ///     Creates an <see cref="AuthorizationFailure" /> that represents the
    ///     absence of configured rules for a given authorization argument type.
    /// </summary>
    /// <param name="authorizationArgsType">
    ///     The authorization argument type for which no rules were configured.
    /// </param>
    /// <param name="message">
    ///     An optional overriding message; if <c>null</c> or whitespace, a generic message is used.
    /// </param>
    /// <returns>
    ///     An <see cref="AuthorizationFailure" /> representing the "no rules" condition.
    /// </returns>
    internal static AuthorizationFailure NoRules(
        Type authorizationArgsType,
        string? message = null)
    {
        var safeMessage = string.IsNullOrWhiteSpace(message)
            ? $"No authorization rules are configured for arguments of type '{authorizationArgsType.FullName}'."
            : message;

        return new AuthorizationFailure(
            safeMessage,
            "NoRulesConfigured",
            typeof(AuthorizationManager));
    }
}