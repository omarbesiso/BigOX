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

    private static int GetVersion(Guid g)
    {
        Span<byte> bytes = stackalloc byte[16];
        g.TryWriteBytes(bytes);
        return (bytes[7] >> 4) & 0x0F;
    }
}