namespace BigOX.Security;

/// <summary>
///     Represents the result of a single authorization rule evaluation.
/// </summary>
public readonly record struct AuthorizationResult
{
    private AuthorizationResult(bool successful, string? message)
    {
        Successful = successful;
        Message = message;
    }

    /// <summary>
    ///     Gets a value indicating whether the authorization rule evaluated successfully.
    /// </summary>
    public bool Successful { get; }

    /// <summary>
    ///     Gets an informative, non-sensitive message about the rule passing or failing.
    ///     Implementations should avoid including secrets, PII, or internal system details
    ///     in this message, as it may be logged or surfaced to callers.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    ///     Creates a successful <see cref="AuthorizationResult" /> instance.
    /// </summary>
    /// <param name="message">
    ///     An optional, non-sensitive informational message describing the successful result.
    /// </param>
    /// <returns>A successful <see cref="AuthorizationResult" />.</returns>
    public static AuthorizationResult Success(string? message = null)
    {
        return new AuthorizationResult(true, message);
    }

    /// <summary>
    ///     Creates a failed <see cref="AuthorizationResult" /> instance.
    /// </summary>
    /// <param name="message">
    ///     A non-empty, non-sensitive message describing the reason for failure.
    ///     If the value is <c>null</c>, empty, or whitespace, a generic message is used.
    /// </param>
    /// <returns>A failed <see cref="AuthorizationResult" />.</returns>
    public static AuthorizationResult Failure(string? message)
    {
        var safeMessage = string.IsNullOrWhiteSpace(message)
            ? "Authorization rule failed."
            : message;

        return new AuthorizationResult(false, safeMessage);
    }
}