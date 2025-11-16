using System.Linq.Expressions;

namespace BigOX.Domain;

/// <summary>
///     Represents a reusable, named business rule or query predicate for a given
///     entity type <typeparamref name="T" />.
/// </summary>
/// <remarks>
///     <para>
///         A specification encapsulates the logic that determines whether a candidate
///         entity satisfies a particular business rule. It can be used both:
///     </para>
///     <list type="bullet">
///         <item>
///             <description>
///                 In-memory, via <see cref="IsSatisfiedBy" />, for validation and
///                 decision-making in the domain layer.
///             </description>
///         </item>
///         <item>
///             <description>
///                 Against a data source, by using <see cref="ToExpression" /> with
///                 LINQ providers such as Entity Framework Core.
///             </description>
///         </item>
///     </list>
///     <para>
///         Typical usage is to create small, strongly-typed specification classes
///         (e.g. <c>ActiveCustomerSpecification</c>) that hard-code domain knowledge
///         and can be composed into more complex rules.
///     </para>
/// </remarks>
/// <typeparam name="T">
///     The type of entity or value object the specification applies to.
/// </typeparam>
public interface ISpecification<T>
{
    /// <summary>
    ///     Converts this specification into a LINQ expression that can be used
    ///     to filter <typeparamref name="T" /> instances.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Implementations should return a pure expression (without side
    ///         effects) so that it can be safely used with LINQ providers, including
    ///         those that translate expressions to other query languages (for
    ///         example, SQL via Entity Framework Core).
    ///     </para>
    ///     <para>
    ///         The returned expression is typically of the form:
    ///         <c>entity =&gt; /* predicate over entity */</c>.
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     An <see cref="Expression{TDelegate}" /> representing the predicate
    ///     that defines this specification.
    /// </returns>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    ///     Evaluates this specification against a single candidate instance.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method is intended for in-memory checks (for example, enforcing
    ///         invariants inside an aggregate, or validating a DTO before
    ///         persistence). Implementations typically delegate to
    ///         <see cref="ToExpression" /> and compile the expression.
    ///     </para>
    ///     <para>
    ///         Because compilation of expressions can be relatively expensive,
    ///         callers that need to evaluate the specification many times in a tight
    ///         loop should consider caching the compiled delegate.
    ///     </para>
    /// </remarks>
    /// <param name="candidate">
    ///     The candidate instance to evaluate.
    /// </param>
    /// <returns>
    ///     <see langword="true" /> if the candidate satisfies the specification;
    ///     otherwise, <see langword="false" />.
    /// </returns>
    bool IsSatisfiedBy(T candidate);
}