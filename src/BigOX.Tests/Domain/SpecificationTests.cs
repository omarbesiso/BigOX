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

    private sealed class GreaterThanZeroSpec : Specification<int>
    {
        public override Expression<Func<int, bool>> ToExpression()
        {
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