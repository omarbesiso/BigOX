using BigOX.Results;
using BigOX.Tests.Validation;

namespace BigOX.Tests.Results;

[TestClass]
public class ErrorTests
{
    [TestMethod]
    public void Create_Should_Default_Code_To_Kind_Value_When_Null_Or_Whitespace()
    {
        var e = Error.Create("msg", null, ErrorKind.Unexpected);
        Assert.AreEqual(ErrorKind.Unexpected.Value, e.Code);
    }

    [TestMethod]
    public void Create_Should_Assign_All_Fields()
    {
        var meta = new Dictionary<string, object?> { ["x"] = 5 };
        var ex = new InvalidOperationException("boom");
        var e = Error.Create("msg", "C1", ErrorKind.Default, ex, meta);
        Assert.AreEqual("msg", e.ErrorMessage);
        Assert.AreEqual("C1", e.Code);
        Assert.AreEqual(ErrorKind.Default, e.Kind);
        Assert.AreEqual(ex, e.Exception);
        Assert.AreEqual(5, e.Metadata["x"]);
    }

    [TestMethod]
    public void Unexpected_Should_Set_Kind_And_Code_Defaults()
    {
        var e = Error.Unexpected("msg");
        Assert.AreEqual(ErrorKind.Unexpected, e.Kind);
        Assert.AreEqual(ErrorKind.Unexpected.Value, e.Code);
    }

    [TestMethod]
    public void Metadata_Should_Be_Frozen_And_Not_Reflect_External_Mutations()
    {
        var meta = new Dictionary<string, object?> { ["a"] = 1 };
        var e = Error.Create("m", metadata: meta);
        meta["a"] = 2; // mutate original
        Assert.AreEqual(1, e.Metadata["a"]);
    }

    [TestMethod]
    public void ErrorKind_Should_Throw_On_Invalid_Value()
    {
        TestUtils.Expect<ArgumentException>(() => _ = new ErrorKind(""));
        TestUtils.Expect<ArgumentException>(() => ErrorKind.FromString(" "));
    }

    [TestMethod]
    public void ErrorKind_Equality_Based_On_Value()
    {
        var a = new ErrorKind("K1");
        var b = new ErrorKind("K1");
        var c = new ErrorKind("K2");
        Assert.IsTrue(a.Equals(b));
        Assert.IsFalse(a.Equals(c));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }
}