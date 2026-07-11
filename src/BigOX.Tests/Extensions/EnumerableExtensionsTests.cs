using System.Collections;
using BigOX.Extensions;
using BigOX.Tests.Validation;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class EnumerableExtensionsTests
{
    [TestMethod]
    public void IsEmpty_NonGeneric_EmptyArray_ReturnsTrue()
    {
        IEnumerable col = Array.Empty<int>();
        Assert.IsTrue(col.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_NonGeneric_NonEmptyList_ReturnsFalse()
    {
        IEnumerable col = new ArrayList { 1, 2, 3 };
        Assert.IsFalse(col.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_NonGeneric_EnumeratorFallback_Works()
    {
        IEnumerable empty = new NonGenericEmptyEnumerable();
        IEnumerable nonEmpty = new NonGenericSingleEnumerable();

        Assert.IsTrue(empty.IsEmpty());
        Assert.IsFalse(nonEmpty.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_NonGeneric_Null_ThrowsArgumentNullException()
    {
        IEnumerable? col = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => col!.IsEmpty());
    }

    [TestMethod]
    public void IsNotEmpty_NonGeneric_Works()
    {
        IEnumerable empty = Array.Empty<int>();
        IEnumerable nonEmpty = new ArrayList { 1 };

        Assert.IsFalse(empty.IsNotEmpty());
        Assert.IsTrue(nonEmpty.IsNotEmpty());
    }

    [TestMethod]
    public void IsEmpty_Generic_EmptyList_ReturnsTrue()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var list = new List<int>();
        Assert.IsTrue(list.IsEmpty());
    }

    [TestMethod]
    public void IsNotEmpty_Generic_NonEmptyList_ReturnsTrue()
    {
        var list = new List<int> { 1 };
        Assert.IsTrue(list.IsNotEmpty());
    }

    [TestMethod]
    public void IsEmpty_Generic_NonEmptyList_ReturnsFalse()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert.IsFalse(list.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_Generic_CountedCollection_UsesCountWithoutEnumerating()
    {
        // IReadOnlyCollection<T> exposes Count, so TryGetNonEnumeratedCount short-circuits.
        IEnumerable<int> empty = new List<int>();
        IEnumerable<int> nonEmpty = new List<int> { 1 };

        Assert.IsTrue(empty.IsEmpty());
        Assert.IsFalse(nonEmpty.IsEmpty());
    }

    [TestMethod]
    public void IsEmpty_Generic_Null_ThrowsArgumentNullException()
    {
        IEnumerable<int>? col = null;
        var ex = TestUtils.Expect<ArgumentNullException>(() => col!.IsEmpty());
        StringAssert.Contains(ex.ParamName, "collection");
    }

    [TestMethod]
    public void IsEmpty_Generic_LazyNonEmptySequence_ConsumesExactlyOneElement()
    {
        var sequence = new CountingEnumerable([1, 2, 3]);

        Assert.IsFalse(sequence.IsEmpty());
        Assert.AreEqual(1, sequence.MoveNextCalls);
    }

    [TestMethod]
    public void IsEmpty_Generic_LazyEmptySequence_ConsumesExactlyOneElement()
    {
        var sequence = new CountingEnumerable([]);

        Assert.IsTrue(sequence.IsEmpty());
        Assert.AreEqual(1, sequence.MoveNextCalls);
    }

    [TestMethod]
    public void IsNullOrEmpty_NonGeneric_Null_ReturnsTrue()
    {
        IEnumerable? col = null;
        Assert.IsTrue(col.IsNullOrEmpty());
    }

    [TestMethod]
    public void IsNullOrEmpty_Generic_Null_ReturnsTrue()
    {
        IEnumerable<int>? col = null;
        Assert.IsTrue(col.IsNullOrEmpty());
    }

    [TestMethod]
    public void IsNotNullOrEmpty_NonGeneric_Null_ReturnsFalse()
    {
        IEnumerable? col = null;
        Assert.IsFalse(col.IsNotNullOrEmpty());
    }

    [TestMethod]
    public void IsNotNullOrEmpty_Generic_NonNullNonEmpty_ReturnsTrue()
    {
        IEnumerable<int> col = new List<int> { 1, 2 };
        Assert.IsTrue(col.IsNotNullOrEmpty());
    }

    [TestMethod]
    public void Chunk_SplitsIntoExpectedChunks()
    {
        var input = Enumerable.Range(1, 9).ToList();

        var chunks = EnumerableExtensions.Chunk(input, 3).Select(c => c.ToArray()).ToArray();

        Assert.HasCount(3, chunks);
        CollectionAssert.AreEqual((int[])[1, 2, 3], chunks[0]);
        CollectionAssert.AreEqual((int[])[4, 5, 6], chunks[1]);
        CollectionAssert.AreEqual((int[])[7, 8, 9], chunks[2]);
    }

    [TestMethod]
    public void Chunk_LastChunkCanBeSmaller()
    {
        var input = Enumerable.Range(1, 5).ToList();

        var chunks = EnumerableExtensions.Chunk(input, 2).Select(c => c.Count()).ToArray();

        CollectionAssert.AreEqual((int[])[2, 2, 1], chunks);
    }

    [TestMethod]
    public void Chunk_ChunkSizeZeroOrNegative_ThrowsArgumentException()
    {
        var input = new[] { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentException>(() => EnumerableExtensions.Chunk(input, 0).ToList());
        Assert.ThrowsExactly<ArgumentException>(() => EnumerableExtensions.Chunk(input, -1).ToList());
    }

    [TestMethod]
    public void Chunk_NullCollection_ThrowsArgumentNullException()
    {
        IEnumerable<int>? input = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => EnumerableExtensions.Chunk(input!, 2).ToList());
    }

    private sealed class NonGenericEmptyEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator() => new EmptyEnumerator();

        private sealed class EmptyEnumerator : IEnumerator
        {
            public object? Current => null;

            public bool MoveNext() => false;

            public void Reset()
            {
            }
        }
    }

    private sealed class NonGenericSingleEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator() => new SingleEnumerator();

        private sealed class SingleEnumerator : IEnumerator
        {
            private bool _moved;
            public object Current => 1;

            public bool MoveNext()
            {
                if (_moved)
                {
                    return false;
                }

                _moved = true;
                return true;
            }

            public void Reset()
            {
                _moved = false;
            }
        }
    }

    /// <summary>
    ///     A lazy sequence that only implements <see cref="IEnumerable{T}" /> (deliberately not
    ///     <see cref="ICollection{T}" />/<see cref="IReadOnlyCollection{T}" />) so that
    ///     <c>TryGetNonEnumeratedCount</c> fails and the enumerator fallback is exercised. Counts how many times
    ///     <c>MoveNext</c> is invoked so tests can assert at most one element is consumed.
    /// </summary>
    private sealed class CountingEnumerable(int[] items) : IEnumerable<int>
    {
        private readonly int[] _items = items;

        public int MoveNextCalls { get; private set; }

        public IEnumerator<int> GetEnumerator() => new CountingEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private sealed class CountingEnumerator(CountingEnumerable owner) : IEnumerator<int>
        {
            private int _index = -1;

            public int Current => owner._items[_index];

            object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                owner.MoveNextCalls++;
                _index++;
                return _index < owner._items.Length;
            }

            public void Reset() => _index = -1;

            public void Dispose()
            {
            }
        }
    }
}