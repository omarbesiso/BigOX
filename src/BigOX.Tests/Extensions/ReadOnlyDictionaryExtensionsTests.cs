using System.Collections.Frozen;
using System.Collections.ObjectModel;
using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class ReadOnlyDictionaryExtensionsTests
{
    [TestMethod]
    public void FreezeOrEmpty_NullSource_ReturnsFrozenEmpty()
    {
        IReadOnlyDictionary<string, object?>? src = null;
        var result = src.FreezeOrEmpty();

        Assert.IsNotNull(result);
        Assert.AreSame(FrozenDictionary<string, object?>.Empty, result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void FreezeOrEmpty_AlreadyFrozen_ReturnsSameInstance()
    {
        var frozen = new Dictionary<string, object?> { ["A"] = 1 }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        var result = frozen.FreezeOrEmpty();

        Assert.IsTrue(ReferenceEquals(frozen, result));
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("a")); // comparer preserved
    }

    [TestMethod]
    public void FreezeOrEmpty_EmptyDictionary_ReturnsFrozenEmptyInstance()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var dict = new Dictionary<string, object?>();
        var result = dict.FreezeOrEmpty();

        Assert.AreSame(FrozenDictionary<string, object?>.Empty, result);
        Assert.IsEmpty(result);
    }

    [TestMethod]
    public void FreezeOrEmpty_Dictionary_PreservesComparer_AndCopiesEntries()
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = 123
        };

        var result = dict.FreezeOrEmpty();

        // Ensure contents copied
        Assert.HasCount(1, result);
        Assert.AreEqual(123, result["a"]);
        // Case-insensitive lookup should succeed (comparer preserved)
        Assert.IsTrue(result.ContainsKey("a"));
        Assert.IsTrue(result.ContainsKey("A"));
    }

    [TestMethod]
    public void FreezeOrEmpty_ReadOnlyWrapper_DoesNotPreserveComparer_FallsBackToDefault()
    {
        // Wrap a case-insensitive dictionary with ReadOnlyDictionary to hide the comparer
        var inner = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = 1
        };
        var wrapped = new ReadOnlyDictionary<string, object?>(inner);

        var result = wrapped.FreezeOrEmpty();

        // Default comparer is Ordinal: lookup by different casing should fail
        Assert.HasCount(1, result);
        Assert.IsTrue(result.ContainsKey("A"));
        Assert.IsFalse(result.ContainsKey("a"));
    }
}