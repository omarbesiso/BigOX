using System.Collections.ObjectModel;
using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class CollectionExtensionsTests
{
    // --------------------
    // Shuffle<T>(IList<T>)
    // --------------------

    [TestMethod]
    public void Shuffle_NullCollection_ThrowsArgumentNullException()
    {
        IList<int>? list = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => list!.Shuffle());
    }

    [TestMethod]
    public void Shuffle_ReadOnly_InPlace_ThrowsNotSupportedException()
    {
        var ro = Array.AsReadOnly([1, 2, 3]);
        Assert.ThrowsExactly<NotSupportedException>(() => ro.Shuffle(false, new AlwaysZeroRandom()));
    }

    [TestMethod]
    public void Shuffle_PreserveOriginal_ReadOnly_SourceUnchanged_ReturnsNewShuffledList()
    {
        var source = Array.AsReadOnly([1, 2, 3, 4]);
        var copy = source.Shuffle(true, new AlwaysZeroRandom());

        // original unchanged and returned instance different
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, source.ToArray());
        Assert.IsFalse(ReferenceEquals(source, copy));
        // deterministic expected order with AlwaysZeroRandom (Fisher–Yates)
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 1 }, copy.ToArray());
    }

    [TestMethod]
    public void Shuffle_InPlace_ModifiesOriginalAndReturnsSameInstance()
    {
        IList<int> list = new List<int> { 1, 2, 3, 4 };
        var result = list.Shuffle(false, new AlwaysZeroRandom());

        Assert.IsTrue(ReferenceEquals(list, result));
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 1 }, list.ToArray());
    }

    [TestMethod]
    public void Shuffle_PreserveOriginal_ReturnsNewInstance_OriginalUnchanged()
    {
        IList<int> list = new List<int> { 10, 20, 30, 40 };
        var result = list.Shuffle(true, new AlwaysZeroRandom());

        Assert.IsFalse(ReferenceEquals(list, result));
        CollectionAssert.AreEqual(new[] { 10, 20, 30, 40 }, list.ToArray());
        CollectionAssert.AreEqual(new[] { 20, 30, 40, 10 }, result.ToArray());
    }

    [TestMethod]
    public void Shuffle_ZeroOrOneElement_NoChange()
    {
        IList<int> empty = new List<int>();
        IList<int> single = new List<int> { 42 };

        var r1 = empty.Shuffle(random: new AlwaysZeroRandom());
        var r2 = single.Shuffle(random: new AlwaysZeroRandom());

        Assert.AreSame(empty, r1);
        Assert.AreSame(single, r2);
        CollectionAssert.AreEqual(Array.Empty<int>(), empty.ToArray());
        CollectionAssert.AreEqual((int[])[42], single.ToArray());
    }

    // --------------------
    // AddUnique
    // --------------------

    [TestMethod]
    public void AddUnique_NullCollection_ThrowsArgumentNullException()
    {
        ICollection<int>? coll = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => coll!.AddUnique(1));
    }

    [TestMethod]
    public void AddUnique_OnHashSet_AddsOnce()
    {
        ICollection<int> set = new HashSet<int> { 1, 2 };
        Assert.IsTrue(set.AddUnique(3));
        Assert.IsFalse(set.AddUnique(3));
        CollectionAssert.AreEquivalent((int[])[1, 2, 3], set.ToArray());
    }

    [TestMethod]
    public void AddUnique_OnList_AddsWhenMissing_AndSkipsDuplicate()
    {
        ICollection<int> list = new List<int> { 5 };
        Assert.IsTrue(list.AddUnique(6));
        Assert.IsFalse(list.AddUnique(6));
        CollectionAssert.AreEqual((int[])[5, 6], list.ToArray());
    }

    [TestMethod]
    public void AddUnique_OnReadOnlyCollection_ThrowsNotSupportedException()
    {
        var ro = Array.AsReadOnly([1, 2]);
        Assert.ThrowsExactly<NotSupportedException>(() => ro.AddUnique(3));
    }

    // --------------------
    // RemoveWhere
    // --------------------

    [TestMethod]
    public void RemoveWhere_NullCollection_ThrowsArgumentNullException()
    {
        ICollection<int>? coll = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => coll!.RemoveWhere(_ => true));
    }

    [TestMethod]
    public void RemoveWhere_NullPredicate_ThrowsArgumentNullException()
    {
        ICollection<int> list = new List<int> { 1, 2, 3 };
        Assert.ThrowsExactly<ArgumentNullException>(() => list.RemoveWhere(null!));
    }

    [TestMethod]
    public void RemoveWhere_ReadOnly_ThrowsNotSupportedException()
    {
        var ro = Array.AsReadOnly([1, 2, 3]);
        Assert.ThrowsExactly<NotSupportedException>(() => ro.RemoveWhere(_ => true));
    }

    [TestMethod]
    public void RemoveWhere_List_UsesRemoveAll_RemovesExpected()
    {
        ICollection<int> list = new List<int> { 1, 2, 3, 4, 5, 6 };
        var removed = list.RemoveWhere(x => x % 2 == 0);
        Assert.AreEqual(3, removed);
        CollectionAssert.AreEqual((int[])[1, 3, 5], list.ToArray());
    }

    [TestMethod]
    public void RemoveWhere_IListNonList_RemovesFromBack_CountMatches()
    {
        // System.Collections.ObjectModel.Collection<T> implements IList<T> but is not List<T>
        ICollection<int> list = new Collection<int> { 1, 2, 3, 4, 5 };
        var removed = list.RemoveWhere(x => x >= 3);
        Assert.AreEqual(3, removed);
        CollectionAssert.AreEqual((int[])[1, 2], list.ToArray());
    }

    [TestMethod]
    public void RemoveWhere_ISet_RemovesExpected()
    {
        ICollection<int> set = new HashSet<int> { 1, 2, 3, 4, 5 };
        var removed = set.RemoveWhere(x => x % 2 == 1);
        Assert.AreEqual(3, removed);
        CollectionAssert.AreEquivalent((int[])[2, 4], set.ToArray());
    }

    [TestMethod]
    public void RemoveWhere_Fallback_LinkedList_RemovesExpected()
    {
        ICollection<int> linked = new LinkedList<int>([1, 2, 3, 4]);
        var removed = linked.RemoveWhere(x => x <= 2);
        Assert.AreEqual(2, removed);
        CollectionAssert.AreEqual((int[])[3, 4], linked.ToArray());
    }

    [TestMethod]
    public void RemoveWhere_EmptyCollection_ReturnsZero()
    {
        ICollection<int> empty = new List<int>();
        var removed = empty.RemoveWhere(_ => true);
        Assert.AreEqual(0, removed);
        CollectionAssert.AreEqual(Array.Empty<int>(), empty.ToArray());
    }

    // --------------------
    // AddIf
    // --------------------

    [TestMethod]
    public void AddIf_NullCollection_ThrowsArgumentNullException()
    {
        ICollection<int>? coll = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => coll!.AddIf(1, _ => true));
    }

    [TestMethod]
    public void AddIf_NullPredicate_ThrowsArgumentNullException()
    {
        ICollection<int> list = new List<int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => list.AddIf(1, null!));
    }

    [TestMethod]
    public void AddIf_PredicateFalse_NoAdd_ReturnsFalse_EvenIfReadOnly()
    {
        var ro = Array.AsReadOnly([1, 2]);
        var result = ro.AddIf(3, _ => false);
        Assert.IsFalse(result);
        CollectionAssert.AreEqual((int[])[1, 2], ro.ToArray());
    }

    [TestMethod]
    public void AddIf_PredicateTrue_AddsAndReturnsTrue()
    {
        ICollection<int> list = new List<int> { 10 };
        var result = list.AddIf(20, _ => true);
        Assert.IsTrue(result);
        CollectionAssert.AreEqual((int[])[10, 20], list.ToArray());
    }

    [TestMethod]
    public void AddIf_PredicateTrue_ReadOnly_ThrowsNotSupportedException()
    {
        var ro = Array.AsReadOnly([1, 2]);
        Assert.ThrowsExactly<NotSupportedException>(() => ro.AddIf(3, _ => true));
    }

    // --------------------
    // ContainsAny
    // --------------------

    [TestMethod]
    public void ContainsAny_Params_NullValues_ReturnsFalse()
    {
        ICollection<int> list = new List<int> { 1, 2, 3 };
        int[]? values = null;
        Assert.IsFalse(list.ContainsAny(values));
    }

    [TestMethod]
    public void ContainsAny_Params_CollectionNull_ValuesNotNull_ThrowsArgumentNullException()
    {
        ICollection<int>? coll = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => coll!.ContainsAny([1, 2, 3]));
    }

    [TestMethod]
    public void ContainsAny_Span_Empty_ReturnsFalse()
    {
        ICollection<int> list = new List<int> { 1, 2, 3 };
        var vals = ReadOnlySpan<int>.Empty;
        Assert.IsFalse(list.ContainsAny(vals));
    }

    [TestMethod]
    public void ContainsAny_Set_ReturnsTrueWhenPresent_OtherwiseFalse()
    {
        ICollection<int> set = new HashSet<int> { 5, 6, 7 };
        Assert.IsTrue(set.ContainsAny([1, 6, 9]));
        Assert.IsFalse(set.ContainsAny([1, 2, 3]));
    }

    [TestMethod]
    public void ContainsAny_List_SmallValuesThreshold_ReturnsExpected()
    {
        ICollection<int> list = new List<int> { 10, 20, 30, 40 };
        // <= 8 values triggers small-values heuristic
        Assert.IsTrue(list.ContainsAny([5, 20, 99]));
        Assert.IsFalse(list.ContainsAny([-1, -2, -3]));
    }

    [TestMethod]
    public void ContainsAny_List_LargeValues_BuildsValueSet_ReturnsExpected()
    {
        ICollection<int> list = new List<int> { 1, 2, 3, 4 };
        // values length > collection count => build lookup path
        var large = Enumerable.Range(10, 20).Concat([3]).ToArray(); // includes a hit '3'
        Assert.IsTrue(list.ContainsAny(large));
        var none = Enumerable.Range(10, 20).ToArray();
        Assert.IsFalse(list.ContainsAny(none));
    }

    // --------------------
    // AddUniqueRange
    // --------------------

    [TestMethod]
    public void AddUniqueRange_NullCollection_ThrowsArgumentNullException()
    {
        ICollection<int>? coll = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => coll!.AddUniqueRange([1, 2]));
    }

    [TestMethod]
    public void AddUniqueRange_NullValues_ReturnsZero_NoChange()
    {
        ICollection<int> list = new List<int> { 1 };
        var added = list.AddUniqueRange(null);
        Assert.AreEqual(0, added);
        CollectionAssert.AreEqual((int[])[1], list.ToArray());
    }

    [TestMethod]
    public void AddUniqueRange_ReadOnly_ThrowsNotSupportedException()
    {
        var ro = Array.AsReadOnly([1, 2]);
        Assert.ThrowsExactly<NotSupportedException>(() => ro.AddUniqueRange([3, 4]));
    }

    [TestMethod]
    public void AddUniqueRange_Set_AddsOnlyNew()
    {
        ICollection<int> set = new HashSet<int> { 1, 2, 3 };
        var added = set.AddUniqueRange([2, 3, 4, 5]);
        Assert.AreEqual(2, added);
        CollectionAssert.AreEquivalent((int[])[1, 2, 3, 4, 5], set.ToArray());
    }

    [TestMethod]
    public void AddUniqueRange_SmallCandidateCount_Path_RespectsContains()
    {
        ICollection<int> list = new List<int> { 10, 20 };
        // <= 8 candidates so uses Contains-path
        var added = list.AddUniqueRange([20, 30, 40]);
        Assert.AreEqual(2, added);
        CollectionAssert.AreEqual((int[])[10, 20, 30, 40], list.ToArray());
    }

    [TestMethod]
    public void AddUniqueRange_LargeCandidates_BuildsLookup_AddsOnlyUnique()
    {
        // Fallback ICollection<T> (LinkedList<T>) + large candidate set
        ICollection<int> coll = new LinkedList<int>([1, 2, 3]);
        var values = Enumerable.Range(1, 20).ToArray(); // includes duplicates of existing
        var added = coll.AddUniqueRange(values);
        Assert.AreEqual(17, added);
        CollectionAssert.AreEquivalent(Enumerable.Range(1, 20).ToArray(), coll.ToArray());
    }

    [TestMethod]
    public void AddUniqueRange_HashSet_PreservesComparer()
    {
        var hs = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "foo" };
        ICollection<string> coll = hs;
        var added = coll.AddUniqueRange(["FOO", "bar"]);
        Assert.AreEqual(1, added);
        Assert.Contains("foo", hs);
        Assert.Contains("BAR", hs); // comparer should treat bar == BAR
    }

    [TestMethod]
    public void AddUniqueRange_ValuesEqualsCollection_SnapshotsToAvoidEnumerationIssues()
    {
        var list = new List<int> { 1, 2, 3 };
        var added = list.AddUniqueRange(list);
        Assert.AreEqual(0, added);
        CollectionAssert.AreEqual((int[])[1, 2, 3], list.ToArray());
    }

    // Helper Random to make Shuffle deterministic
    private sealed class AlwaysZeroRandom : Random
    {
        public override int Next(int minValue, int maxValue)
        {
            return minValue;
            // always choose the first index
        }
    }
}