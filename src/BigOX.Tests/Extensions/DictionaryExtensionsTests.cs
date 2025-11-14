using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class DictionaryExtensionsTests
{
    // --------------------
    // ToSortedDictionary()
    // --------------------

    [TestMethod]
    public void ToSortedDictionary_NullDictionary_ThrowsArgumentNullException()
    {
        IDictionary<string, int>? dict = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => dict!.ToSortedDictionary());
    }

    [TestMethod]
    public void ToSortedDictionary_ReturnsSortedCopy_OriginalUnchanged()
    {
        IDictionary<string, int> dict = new Dictionary<string, int>
        {
            ["b"] = 2,
            ["a"] = 1,
            ["c"] = 3
        };

        var sorted = dict.ToSortedDictionary();

        // new instance returned
        Assert.IsFalse(ReferenceEquals(dict, sorted));
        // original unchanged
        Assert.HasCount(3, dict);
        // sorted ascending by key
        CollectionAssert.AreEqual((string[])["a", "b", "c"], sorted.Keys.ToArray());
        CollectionAssert.AreEqual((int[])[1, 2, 3], sorted.Values.ToArray());
    }

    [TestMethod]
    public void ToSortedDictionary_WithComparer_NullComparer_ThrowsArgumentNullException()
    {
        IDictionary<string, int> dict = new Dictionary<string, int> { ["a"] = 1 };
        Assert.ThrowsExactly<ArgumentNullException>(() => dict.ToSortedDictionary(null!));
    }

    [TestMethod]
    public void ToSortedDictionary_WithComparer_UsesProvidedComparer()
    {
        IDictionary<string, int> dict = new Dictionary<string, int>
        {
            ["b"] = 2,
            ["A"] = 1,
            ["c"] = 3
        };

        var cmp = StringComparer.OrdinalIgnoreCase;
        var sorted = dict.ToSortedDictionary(cmp);

        // comparer propagated
        Assert.AreSame(cmp, sorted.Comparer);
        // ordering per comparer
        CollectionAssert.AreEqual(new[] { "A", "b", "c" }, sorted.Keys.ToArray());
    }

    // --------------------
    // RemoveWhere
    // --------------------

    [TestMethod]
    public void RemoveWhere_NullDictionary_ThrowsArgumentNullException()
    {
        IDictionary<int, string>? dict = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => dict!.RemoveWhere(_ => true));
    }

    [TestMethod]
    public void RemoveWhere_NullPredicate_ThrowsArgumentNullException()
    {
        IDictionary<int, string> dict = new Dictionary<int, string>();
        Assert.ThrowsExactly<ArgumentNullException>(() => dict.RemoveWhere(null!));
    }

    [TestMethod]
    public void RemoveWhere_EmptyDictionary_NoChange()
    {
        IDictionary<int, string> dict = new Dictionary<int, string>();
        dict.RemoveWhere(_ => true);
        Assert.IsEmpty(dict);
    }

    [TestMethod]
    public void RemoveWhere_RemovesMatchingEntries()
    {
        IDictionary<int, string> dict = new Dictionary<int, string>
        {
            [1] = "one",
            [2] = "two",
            [3] = "three",
            [4] = "four"
        };

        // remove odd keys
        dict.RemoveWhere(kvp => kvp.Key % 2 == 1);

        CollectionAssert.AreEquivalent(new[] { 2, 4 }, dict.Keys.ToArray());
        CollectionAssert.AreEquivalent(new[] { "two", "four" }, dict.Values.ToArray());
    }

    // --------------------
    // Merge
    // --------------------

    [TestMethod]
    public void Merge_NullDictionary_ThrowsArgumentNullException()
    {
        IDictionary<string, int>? dict = null;
        var other = new Dictionary<string, int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => dict!.Merge(other));
    }

    [TestMethod]
    public void Merge_NullOtherDictionary_ThrowsArgumentNullException()
    {
        IDictionary<string, int> dict = new Dictionary<string, int>();
        Assert.ThrowsExactly<ArgumentNullException>(() => dict.Merge(null!));
    }

    [TestMethod]
    public void Merge_OverwriteExistingTrue_ReplacesValuesAndAddsMissing()
    {
        IDictionary<string, int> d1 = new Dictionary<string, int>
        {
            ["One"] = 1,
            ["Two"] = 2
        };
        var d2 = new Dictionary<string, int>
        {
            ["Two"] = 22,
            ["Three"] = 3
        };

        d1.Merge(d2, true);

        Assert.HasCount(3, d1);
        Assert.AreEqual(1, d1["One"]);
        Assert.AreEqual(22, d1["Two"]);
        Assert.AreEqual(3, d1["Three"]);
    }

    [TestMethod]
    public void Merge_OverwriteExistingFalse_KeepsExistingAndAddsMissing()
    {
        IDictionary<string, int> d1 = new Dictionary<string, int>
        {
            ["One"] = 1,
            ["Two"] = 2
        };
        var d2 = new Dictionary<string, int>
        {
            ["Two"] = 22,
            ["Three"] = 3
        };

        d1.Merge(d2, false);

        Assert.HasCount(3, d1);
        Assert.AreEqual(2, d1["Two"]); // existing retained
        Assert.AreEqual(3, d1["Three"]); // new added
    }
}