using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class TypeExtensionsTests
{
    // ---------- IsNumeric ----------
    [TestMethod]
    public void IsNumeric_Primitives_AreNumeric()
    {
        Assert.IsTrue(typeof(byte).IsNumeric());
        Assert.IsTrue(typeof(sbyte).IsNumeric());
        Assert.IsTrue(typeof(short).IsNumeric());
        Assert.IsTrue(typeof(ushort).IsNumeric());
        Assert.IsTrue(typeof(int).IsNumeric());
        Assert.IsTrue(typeof(uint).IsNumeric());
        Assert.IsTrue(typeof(long).IsNumeric());
        Assert.IsTrue(typeof(ulong).IsNumeric());
        Assert.IsTrue(typeof(float).IsNumeric());
        Assert.IsTrue(typeof(double).IsNumeric());
        Assert.IsTrue(typeof(decimal).IsNumeric());
    }

    [TestMethod]
    public void IsNumeric_Nullables_RespectFlag()
    {
        // includeNullableTypes = true -> treat as numeric
        Assert.IsTrue(typeof(int?).IsNumeric());
        Assert.IsTrue(typeof(decimal?).IsNumeric());
        // includeNullableTypes = false -> nullable should be treated as non-numeric (TypeCode.Object)
        Assert.IsFalse(typeof(int?).IsNumeric(false));
    }

    [TestMethod]
    public void IsNumeric_Arrays_And_Enums_ReturnFalse()
    {
        Assert.IsFalse(typeof(int[]).IsNumeric());
        Assert.IsFalse(typeof(ConsoleColor).IsNumeric());
    }

    // ---------- IsOpenGeneric ----------
    [TestMethod]
    public void IsOpenGeneric_GenericDefinition_IsTrue_ClosedIsFalse()
    {
        Assert.IsTrue(typeof(Dictionary<,>).IsOpenGeneric());
        Assert.IsFalse(typeof(Dictionary<string, int>).IsOpenGeneric());
        Assert.IsFalse(typeof(string).IsOpenGeneric());
    }

    [TestMethod]
    public void HasAttribute_ByType_DetectsPresence()
    {
        Assert.IsTrue(typeof(Decorated).HasAttribute(typeof(SampleAttribute)));
        Assert.IsFalse(typeof(NotDecorated).HasAttribute(typeof(SampleAttribute)));
    }

    [TestMethod]
    public void HasAttribute_Generic_WithPredicate_FiltersByProperty()
    {
        Assert.IsTrue(typeof(Decorated).HasAttribute<SampleAttribute>(a => a.Name == "A"));
        Assert.IsFalse(typeof(Decorated).HasAttribute<SampleAttribute>(a => a.Name == "B"));
    }

    // ---------- DefaultValue / DefaultValueAsync ----------
    [TestMethod]
    public void DefaultValue_ReturnsLanguageDefaults()
    {
        Assert.AreEqual(0, typeof(int).DefaultValue());
        Assert.IsNull(typeof(int?).DefaultValue());
        Assert.IsNull(typeof(string).DefaultValue());
        Assert.IsFalse((bool?)typeof(bool).DefaultValue());
        Assert.IsNull(typeof(void).DefaultValue());
    }

    [TestMethod]
    public async Task DefaultValueAsync_CompletesSynchronously_WithSameResult()
    {
        var v1 = await typeof(int).DefaultValueAsync();
        var v2 = await typeof(string).DefaultValueAsync();
        Assert.AreEqual(0, v1);
        Assert.IsNull(v2);
    }

    [TestMethod]
    public void DefaultValue_IsCached_PerType()
    {
        var t = typeof(Guid);
        var v1 = t.DefaultValue();
        var v2 = t.DefaultValue();
        // Default of Guid is default(Guid) -> 0000... should be equal, and caching returns same boxed object instance
        Assert.AreEqual(v1, v2);
        Assert.AreSame(v1, v2);
    }

    // ---------- GetTypeAsString ----------
    [TestMethod]
    public void GetTypeAsString_ReturnsAliases_ForKnownTypes()
    {
        Assert.AreEqual("int", typeof(int).GetTypeAsString());
        Assert.AreEqual("int?", typeof(int?).GetTypeAsString());
        Assert.AreEqual("bool", typeof(bool).GetTypeAsString());
        Assert.AreEqual("string", typeof(string).GetTypeAsString());
        Assert.AreEqual("DateTime", typeof(DateTime).GetTypeAsString());
        Assert.AreEqual("Guid", typeof(Guid).GetTypeAsString());
        Assert.AreEqual("void", typeof(void).GetTypeAsString());
    }

    [TestMethod]
    public void GetTypeAsString_FallbacksToName_WhenNoAlias()
    {
        Assert.AreEqual(nameof(CustomType), typeof(CustomType).GetTypeAsString());
    }

    // ---------- IsNullable ----------
    [TestMethod]
    public void IsNullable_DetectsNullable()
    {
        Assert.IsTrue(typeof(int?).IsNullable());
        Assert.IsFalse(typeof(int).IsNullable());
        Assert.IsFalse(typeof(string).IsNullable()); // reference types are not Nullable<T>
    }

    // ---------- IsOfNullableType<T> ----------
    [TestMethod]
    public void IsOfNullableType_Generic_ReturnsCorrect_ForT()
    {
        Assert.IsTrue(TypeExtensions.IsOfNullableType<int?>());
        Assert.IsFalse(TypeExtensions.IsOfNullableType<int>());
        Assert.IsFalse(TypeExtensions.IsOfNullableType<string>());
    }

    [TestMethod]
    public void IsOfNullableType_OnInstance_Behavior_ForValueTypes()
    {
        var v = 0;
        Assert.IsFalse(v.IsOfNullableType());

        int? maybe = null;
        Assert.ThrowsExactly<ArgumentNullException>(() => maybe!.IsOfNullableType());
    }

    // ---------- HasAttribute ----------
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    private sealed class SampleAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    [Sample("A")]
    private sealed class Decorated
    {
    }

    private sealed class NotDecorated;

    private sealed class CustomType;
}