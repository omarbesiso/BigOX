using BigOX.Validation;

// ReSharper disable PossibleMultipleEnumeration

namespace BigOX.Tests.Validation;

[TestClass]
// ReSharper disable once InconsistentNaming
public class GuardTests_CollectionsAndGuid
{
    [TestMethod]
    public void NotNullOrEmpty_Collection_Throws_OnNull()
    {
        IEnumerable<int>? list = null;
        var ex = TestUtils.Expect<ArgumentNullException>(() => Guard.NotNullOrEmpty(list));
        StringAssert.Contains(ex.ParamName, nameof(list));
    }

    [TestMethod]
    public void NotNullOrEmpty_Collection_Throws_OnEmpty()
    {
        IEnumerable<int> list = Array.Empty<int>();
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.NotNullOrEmpty(list));
        StringAssert.Contains(ex.ParamName, nameof(list));
    }

    [TestMethod]
    public void NotNullOrEmpty_Collection_Returns_OnNonEmpty()
    {
        IEnumerable<int> list = [1];
        var result = Guard.NotNullOrEmpty(list);
        Assert.AreSame(list, result);
        CollectionAssert.AreEqual(list.ToList(), result.ToList());
    }

    [TestMethod]
    public void NotNullOrEmpty_NullableGuid_Throws_OnNull()
    {
        Guid? id = null;
        var ex = TestUtils.Expect<ArgumentNullException>(() => Guard.NotNullOrEmpty(id));
        StringAssert.Contains(ex.ParamName, nameof(id));
    }

    [TestMethod]
    public void NotNullOrEmpty_NullableGuid_Throws_OnEmpty()
    {
        Guid? id = Guid.Empty;
        var ex = TestUtils.Expect<ArgumentException>(() => Guard.NotNullOrEmpty(id));
        StringAssert.Contains(ex.ParamName, nameof(id));
    }

    [TestMethod]
    public void NotNullOrEmpty_NullableGuid_Returns_OnValid()
    {
        Guid? id = Guid.NewGuid();
        var result = Guard.NotNullOrEmpty(id);
        Assert.AreEqual(id.Value, result);
    }

    [TestMethod]
    public void NotEmpty_Guid_Throws_OnEmpty()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.NotEmpty(Guid.Empty));
    }

    [TestMethod]
    public void NotEmpty_Guid_Returns_OnNonEmpty()
    {
        var id = Guid.NewGuid();
        var result = Guard.NotEmpty(id);
        Assert.AreEqual(id, result);
    }

    [TestMethod]
    public void NotDefault_Throws_ForDefaultStructs()
    {
        TestUtils.Expect<ArgumentException>(() => Guard.NotDefault(Guid.Empty));
        TestUtils.Expect<ArgumentException>(() => Guard.NotDefault(default(DateTime)));
        TestUtils.Expect<ArgumentException>(() => Guard.NotDefault(0));
    }

    [TestMethod]
    public void NotDefault_Returns_ForNonDefaultStructs()
    {
        Assert.AreNotEqual(Guid.Empty, Guard.NotDefault(Guid.NewGuid()));
        Assert.AreEqual(1, Guard.NotDefault(1));
    }
}