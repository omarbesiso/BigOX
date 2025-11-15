using System.Collections.ObjectModel;
using BigOX.Cqrs;

namespace BigOX.Tests.Cqrs;

[TestClass]
public sealed class ReflectionExtensionsTests
{
    [TestMethod]
    public void IsBasedOn_NonGeneric_Assignable_ReturnsTrue()
    {
        Assert.IsTrue(typeof(Derived).IsBasedOn(typeof(Base)));
        Assert.IsTrue(typeof(string).IsBasedOn(typeof(object)));
    }

    [TestMethod]
    public void IsBasedOn_NonGeneric_Unrelated_ReturnsFalse()
    {
        Assert.IsFalse(typeof(string).IsBasedOn(typeof(IFormatProvider)));
        Assert.IsFalse(typeof(int).IsBasedOn(typeof(IDisposable)));
    }

    // ---------- Open generic interface assignability ----------
    [TestMethod]
    public void IsBasedOn_OpenGeneric_Interface_Implemented_ReturnsTrue()
    {
        Assert.IsTrue(typeof(List<int>).IsBasedOn(typeof(IEnumerable<>)));
        Assert.IsTrue(typeof(string).IsBasedOn(typeof(IEnumerable<>))); // string : IEnumerable<char>
    }

    [TestMethod]
    public void IsBasedOn_OpenGeneric_Interface_NotImplemented_ReturnsFalse()
    {
        Assert.IsFalse(typeof(List<int>).IsBasedOn(typeof(IDictionary<,>)));
        Assert.IsFalse(typeof(int).IsBasedOn(typeof(IEnumerable<>)));
    }

    // ---------- Open generic base/self assignability ----------
    [TestMethod]
    public void IsBasedOn_OpenGeneric_Self_GenericDefinition_ReturnsTrue()
    {
        Assert.IsTrue(typeof(Task<string>).IsBasedOn(typeof(Task<>)));
    }

    [TestMethod]
    public void IsBasedOn_OpenGeneric_ThroughBaseType_ReturnsTrue()
    {
        // SpecialCollection : Collection<int> : IList<int> : IEnumerable<int>
        Assert.IsTrue(typeof(SpecialCollection).IsBasedOn(typeof(Collection<>)));
        Assert.IsTrue(typeof(SpecialCollection).IsBasedOn(typeof(IEnumerable<>)));
    }

    // ---------- Closed generic interface assignability ----------
    [TestMethod]
    public void IsBasedOn_ClosedGeneric_Interface_ReturnsTrue()
    {
        Assert.IsTrue(typeof(List<int>).IsBasedOn(typeof(IEnumerable<int>)));
    }

    // ---------- Null parameter behavior ----------
    [TestMethod]
    public void IsBasedOn_NullOtherType_ThrowsNullReference()
    {
        Assert.ThrowsExactly<NullReferenceException>(() => typeof(int).IsBasedOn(null!));
    }

    // ---------- Non-generic assignability ----------
    private class Base
    {
    }

    private class Derived : Base
    {
    }

    private class SpecialCollection : Collection<int>
    {
    }
}