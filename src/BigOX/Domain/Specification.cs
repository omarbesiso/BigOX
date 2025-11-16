using System.Linq.Expressions;

namespace BigOX.Domain
{
    /// <summary>
    /// Base class for specifications that need only provide a LINQ expression.
    /// </summary>
    /// <typeparam name="T">
    /// The type of entity or value object the specification applies to.
    /// </typeparam>
    public abstract class Specification<T> : ISpecification<T>
    {
        /// <inheritdoc />
        public abstract Expression<Func<T, bool>> ToExpression();

        /// <inheritdoc />
        public virtual bool IsSatisfiedBy(T candidate)
        {
            if (candidate is null)
                throw new ArgumentNullException(nameof(candidate));

            // Note: cache compiled delegates outside this method if you call it in hot paths.
            var predicate = ToExpression().Compile();
            return predicate(candidate);
        }
    }
}