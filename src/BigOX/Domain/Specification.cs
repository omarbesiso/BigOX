using System.Linq.Expressions;

namespace BigOX.Domain;

/// <summary>
///     Base class for specifications that need only provide a LINQ expression.
/// </summary>
/// <typeparam name="T">
///     The type of entity or value object the specification applies to.
/// </typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private Func<T, bool>? _compiledPredicate;

    /// <inheritdoc />
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="candidate" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     The delegate produced by <see cref="ToExpression" /> is compiled once on first use and cached for the lifetime
    ///     of this instance, so repeated calls avoid recompiling the expression tree. This relies on
    ///     <see cref="ToExpression" /> being pure and returning a stable expression (its documented contract); a subclass
    ///     that must vary its expression dynamically should override <see cref="IsSatisfiedBy" /> instead. A benign race
    ///     in which two threads each compile the predicate once is possible and harmless, so no locking is used.
    /// </remarks>
    public virtual bool IsSatisfiedBy(T candidate)
    {
        if (candidate is null)
        {
            throw new ArgumentNullException(nameof(candidate));
        }

        _compiledPredicate ??= ToExpression().Compile();
        return _compiledPredicate(candidate);
    }
}