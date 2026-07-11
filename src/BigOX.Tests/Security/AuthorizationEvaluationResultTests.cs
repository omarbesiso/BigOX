using System.Collections;
using BigOX.Security;

namespace BigOX.Tests.Security;

[TestClass]
public sealed class AuthorizationEvaluationResultTests
{
    [TestMethod]
    public void Failed_NullFailures_Throws_ArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => AuthorizationEvaluationResult.Failed(null!));
    }

    [TestMethod]
    public void Failed_EmptyReadOnlyList_Throws_ArgumentException()
    {
        // An empty IReadOnlyList that is not an ICollection<T> previously slipped through the
        // guard's deferred emptiness check and produced a "failed" result with zero failures.
        IReadOnlyList<AuthorizationFailure> failures = new ReadOnlyListView<AuthorizationFailure>([]);
        Assert.ThrowsExactly<ArgumentException>(() => AuthorizationEvaluationResult.Failed(failures));
    }

    // A read-only collection that deliberately does NOT implement ICollection<T>.
    private sealed class ReadOnlyListView<T>(IReadOnlyList<T> items) : IReadOnlyList<T>
    {
        public T this[int index] => items[index];
        public int Count => items.Count;
        public IEnumerator<T> GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
