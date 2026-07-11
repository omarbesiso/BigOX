using BigOX.Factories;

namespace BigOX.Tests.Factories;

[TestClass]
public sealed class GuidFactoryTests
{
    [TestMethod]
    public void NewSequentialGuid_ReturnsVersion7Guid_UniquePerCall()
    {
        var g1 = GuidFactory.NewSequentialGuid();
        var g2 = GuidFactory.NewSequentialGuid();

        Assert.AreNotEqual(Guid.Empty, g1);
        Assert.AreNotEqual(Guid.Empty, g2);
        Assert.AreNotEqual(g1, g2);

        // Version is in the 7th nibble: (g >> 76) & 0xF == 7
        Span<byte> bytes = stackalloc byte[16];
        g1.TryWriteBytes(bytes);
        var version = (bytes[7] >> 4) & 0x0F;
        Assert.AreEqual(7, version);
    }

    [TestMethod]
    public void NewSequentialGuids_WithPositiveCount_YieldsRequestedAmount_AllUnique_Version7()
    {
        const int count = 10;
        var list = GuidFactory.NewSequentialGuids(count).ToList();

        Assert.HasCount(count, list);
        CollectionAssert.AllItemsAreUnique(list);
        Assert.IsTrue(list.All(g => GetVersion(g) == 7));
    }

    [TestMethod]
    public void NewSequentialGuids_CountLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => GuidFactory.NewSequentialGuids(0).ToList());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => GuidFactory.NewSequentialGuids(-5).ToList());
    }

    [TestMethod]
    public void NewSequentialGuid_WithTimestamp_ProducesVersion7()
    {
        var g = GuidFactory.NewSequentialGuid(DateTimeOffset.UnixEpoch.AddDays(1));
        Assert.AreEqual(7, GetVersion(g));
    }

    [TestMethod]
    public void NewSequentialGuid_IncreasingTimestamps_ProduceAscendingTimeOrderedGuids()
    {
        var earlier = GuidFactory.NewSequentialGuid(new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var later = GuidFactory.NewSequentialGuid(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        Assert.IsLessThan(0, CompareTimestampBytes(earlier, later));
        Assert.IsGreaterThan(0, CompareTimestampBytes(later, earlier));
    }

    [TestMethod]
    public void NewSequentialGuid_TimestampBeforeUnixEpoch_ThrowsArgumentOutOfRangeException()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            () => GuidFactory.NewSequentialGuid(DateTimeOffset.UnixEpoch.AddMilliseconds(-1)));
    }

    [TestMethod]
    public void NewSequentialGuids_Span_FillsAllSlotsUniqueVersion7()
    {
        Span<Guid> ids = stackalloc Guid[8];
        GuidFactory.NewSequentialGuids(ids);

        var array = ids.ToArray();
        Assert.IsTrue(array.All(g => g != Guid.Empty));
        Assert.IsTrue(array.All(g => GetVersion(g) == 7));
        CollectionAssert.AllItemsAreUnique(array);
    }

    [TestMethod]
    public void NewSequentialGuids_EmptySpan_IsNoOp()
    {
        Span<Guid> empty = Span<Guid>.Empty;
        GuidFactory.NewSequentialGuids(empty);
        Assert.AreEqual(0, empty.Length);
    }

    private static int GetVersion(Guid g)
    {
        Span<byte> bytes = stackalloc byte[16];
        g.TryWriteBytes(bytes);
        return (bytes[7] >> 4) & 0x0F;
    }

    private static int CompareTimestampBytes(Guid a, Guid b)
    {
        Span<byte> first = stackalloc byte[16];
        Span<byte> second = stackalloc byte[16];

        // bigEndian:true renders the RFC layout, so the leading 48 bits are the time-ordered prefix.
        a.TryWriteBytes(first, bigEndian: true, out _);
        b.TryWriteBytes(second, bigEndian: true, out _);

        return first[..6].SequenceCompareTo(second[..6]);
    }
}