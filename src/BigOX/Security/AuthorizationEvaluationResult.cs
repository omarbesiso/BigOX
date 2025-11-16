using System.Security;
using BigOX.Validation;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace BigOX.Security;

/// <summary>
///     Represents the aggregated result of evaluating all authorization rules
///     for a given operation.
/// </summary>
public readonly record struct AuthorizationEvaluationResult
{
    private AuthorizationEvaluationResult(
        bool isSuccessful,
        bool hasRules,
        IReadOnlyList<AuthorizationFailure> failures)
    {
        IsSuccessful = isSuccessful;
        HasRules = hasRules;
        Failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    /// <summary>
    ///     Gets a value indicating whether all evaluated rules were successful.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    ///     Gets a value indicating whether any rules were evaluated.
    /// </summary>
    public bool HasRules { get; }

    /// <summary>
    ///     Gets the collection of authorization failures.
    ///     The collection is empty when <see cref="IsSuccessful" /> is <c>true</c>.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public IReadOnlyList<AuthorizationFailure> Failures { get; }

    /// <summary>
    ///     Creates a successful evaluation result.
    /// </summary>
    /// <param name="hasRules">
    ///     Specifies whether any rules were actually evaluated.
    /// </param>
    /// <returns>
    ///     A successful <see cref="AuthorizationEvaluationResult" />.
    /// </returns>
    public static AuthorizationEvaluationResult Success(bool hasRules = true)
    {
        return new AuthorizationEvaluationResult(true, hasRules, Array.Empty<AuthorizationFailure>());
    }

    /// <summary>
    ///     Creates a failed evaluation result.
    /// </summary>
    /// <param name="failures">
    ///     The collection of failures that occurred during evaluation.
    ///     Must not be <c>null</c> or empty.
    /// </param>
    /// <param name="hasRules">
    ///     Specifies whether any rules were actually evaluated.
    ///     Typically <c>true</c> for failure results.
    /// </param>
    /// <returns>
    ///     A failed <see cref="AuthorizationEvaluationResult" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="failures" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when <paramref name="failures" /> is empty.
    /// </exception>
    public static AuthorizationEvaluationResult Failed(
        IReadOnlyList<AuthorizationFailure> failures,
        bool hasRules = true)
    {
        Guard.NotNullOrEmpty(failures);
        return new AuthorizationEvaluationResult(false, hasRules, failures);
    }

    /// <summary>
    ///     Creates a failed evaluation result that represents the absence
    ///     of configured rules for the given authorization argument type.
    /// </summary>
    /// <param name="authorizationArgsType">
    ///     The authorization argument type for which no rules were configured.
    /// </param>
    /// <returns>
    ///     A failed <see cref="AuthorizationEvaluationResult" /> with a single
    ///     <see cref="AuthorizationFailure" /> representing the "no rules" condition.
    /// </returns>
    internal static AuthorizationEvaluationResult NoRulesDeny(Type authorizationArgsType)
    {
        var failure = AuthorizationFailure.NoRules(authorizationArgsType);
        return Failed([failure], false);
    }

    /// <summary>
    ///     Creates a <see cref="SecurityException" /> that represents the failures
    ///     in this evaluation result.
    /// </summary>
    /// <returns>
    ///     A <see cref="SecurityException" /> containing all failure information.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="IsSuccessful" /> is <c>true</c>.
    /// </exception>
    public SecurityException ToSecurityException()
    {
        if (IsSuccessful)
        {
            throw new InvalidOperationException(
                "Cannot create a SecurityException from a successful authorization result.");
        }

        if (Failures.Count == 1)
        {
            var failure = Failures[0];
            return new SecurityException(failure.Message);
        }

        var innerExceptions = new SecurityException[Failures.Count];
        for (var i = 0; i < Failures.Count; i++)
        {
            innerExceptions[i] = new SecurityException(Failures[i].Message);
        }

        var aggregate = new AggregateException(innerExceptions.ToList());

        return new SecurityException(
            "Multiple authorization rules failed. See the inner exception for details.",
            aggregate);
    }
}