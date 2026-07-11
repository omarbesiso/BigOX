using System.Linq.Expressions;
using BigOX.Domain;
using BigOX.Tests.Validation;

namespace BigOX.Tests.Domain;

[TestClass]
public sealed class SpecificationTests
{
    [TestMethod]
    public void IsSatisfiedBy_ReturnsTrue_WhenPredicateMatches_IntGreaterThanZero()
    {
        var spec = new GreaterThanZeroSpec();
        Assert.IsTrue(spec.IsSatisfiedBy(1));
    }

    [TestMethod]
    public void IsSatisfiedBy_ReturnsFalse_WhenPredicateDoesNotMatch_IntGreaterThanZero()
    {
        var spec = new GreaterThanZeroSpec();
        Assert.IsFalse(spec.IsSatisfiedBy(0));
        Assert.IsFalse(spec.IsSatisfiedBy(-5));
    }

    [TestMethod]
    public void IsSatisfiedBy_ThrowsArgumentNullException_WhenReferenceCandidateIsNull()
    {
        var spec = new NonEmptyStringSpec();
        TestUtils.Expect<ArgumentNullException>(
            () => spec.IsSatisfiedBy(null!),
            ex => Assert.AreEqual("candidate", ex.ParamName));
    }

    [TestMethod]
    public void ToExpression_CompilesAndEvaluates_AsExpected()
    {
        var spec = new StringLengthGreaterThanSpec(3);
        var predicate = spec.ToExpression().Compile();
        Assert.IsTrue(predicate("abcd"));
        Assert.IsFalse(predicate("abc"));
    }

    [TestMethod]
    public void IsSatisfiedBy_SameInstance_ProducesConsistentResults()
    {
        var spec = new GreaterThanZeroSpec();

        Assert.IsTrue(spec.IsSatisfiedBy(1));
        Assert.IsTrue(spec.IsSatisfiedBy(2));
        Assert.IsFalse(spec.IsSatisfiedBy(0));
        Assert.IsFalse(spec.IsSatisfiedBy(-3));
    }

    [TestMethod]
    public void IsSatisfiedBy_CompilesExpressionOnce_AcrossManyCalls()
    {
        var spec = new CountingSpec();

        for (var i = 0; i < 10; i++)
        {
            Assert.IsTrue(spec.IsSatisfiedBy(5));
        }

        Assert.AreEqual(1, spec.ToExpressionCallCount);
    }

    private sealed class GreaterThanZeroSpec : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression()
        {
            return x => x > 0;
        }
    }

    private sealed class CountingSpec : Specification<int>
    {
        public int ToExpressionCallCount { get; private set; }

        public override Expression<Func<int, bool>> ToExpression()
        {
            ToExpressionCallCount++;
            return x => x > 0;
        }
    }

    private sealed class NonEmptyStringSpec : Specification<string>
    {
        public override Expression<Func<string, bool>> ToExpression()
        {
            return s => s.Length > 0;
        }
    }

    private sealed class StringLengthGreaterThanSpec(int threshold) : Specification<string>
    {
        public override Expression<Func<string, bool>> ToExpression()
        {
            return s => s.Length > threshold;
        }
    }
}