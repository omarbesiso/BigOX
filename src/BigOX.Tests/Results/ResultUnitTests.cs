using BigOX.Results;

namespace BigOX.Tests.Results;

[TestClass]
public sealed class ResultUnitTests
{
    [TestMethod]
    public void Match_Success_InvokesOnSuccess()
    {
        var r = Result.Success("ok");
        var output = r.Match(() => "s", errs => "f:" + errs.Count);
        Assert.AreEqual("s", output);
    }

    [TestMethod]
    public void Match_Failure_InvokesOnFailure()
    {
        var r = Result.Failure(Error.Create("bad"));
        var output = r.Match(() => "s", errs => "f:" + errs.Count);
        Assert.AreEqual("f:1", output);
    }

    [TestMethod]
    public void Match_Uninitialized_ThrowsInvalidOperationException()
    {
        var r = default(Result);
        Assert.ThrowsExactly<InvalidOperationException>(() => r.Match(() => 1, _ => 2));
    }

    [TestMethod]
    public void Match_NullOnSuccess_ThrowsArgumentNullException()
    {
        var r = Result.Failure(Error.Create("bad"));
        Assert.ThrowsExactly<ArgumentNullException>(() => r.Match<int>(null!, _ => 0));
    }

    [TestMethod]
    public void Match_NullOnFailure_ThrowsArgumentNullException()
    {
        var r = Result.Success();
        Assert.ThrowsExactly<ArgumentNullException>(() => r.Match<int>(() => 0, null!));
    }

    [TestMethod]
    public void Deconstruct_Success_IsSuccessTrueErrorsNull()
    {
        var r = Result.Success();
        var (isSuccess, errors) = r;
        Assert.IsTrue(isSuccess);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void Deconstruct_Failure_IsSuccessFalseWithErrors()
    {
        var r = Result.Failure(Error.Create("bad"));
        var (isSuccess, errors) = r;
        Assert.IsFalse(isSuccess);
        Assert.IsNotNull(errors);
        Assert.HasCount(1, errors!);
    }

    [TestMethod]
    public void Deconstruct_Uninitialized_IsSuccessFalseErrorsNull()
    {
        var r = default(Result);
        var (isSuccess, errors) = r;
        Assert.IsFalse(isSuccess);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetErrors_Failure_ReturnsTrueWithErrors()
    {
        var r = Result.Failure(Error.Create("bad", "B"));
        Assert.IsTrue(r.TryGetErrors(out var errors));
        Assert.IsNotNull(errors);
        Assert.AreEqual("B", errors![0].Code);
    }

    [TestMethod]
    public void TryGetErrors_Success_ReturnsFalse()
    {
        var r = Result.Success();
        Assert.IsFalse(r.TryGetErrors(out var errors));
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void TryGetErrors_Uninitialized_ReturnsFalse()
    {
        var r = default(Result);
        Assert.IsFalse(r.TryGetErrors(out var errors));
        Assert.IsNull(errors);
    }
}
