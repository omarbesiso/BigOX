using BigOX.Results;
using BigOX.Tests.Validation;

namespace BigOX.Tests.Results;

[TestClass]
public class ResultTests
{
    [TestMethod]
    public void Success_Result_Should_Have_Success_Status_And_Value()
    {
        var r = Result<int>.Success(42, "ok");
        Assert.AreEqual(ResultStatus.Success, r.Status);
        Assert.IsTrue(r.IsSuccess(out var value));
        Assert.AreEqual(42, value);
        Assert.IsFalse(r.IsFailure(out var errors));
        Assert.IsNull(errors);
        Assert.AreEqual("ok", r.Message);
    }

    [TestMethod]
    public void Failure_Result_Should_Have_Failure_Status_And_Errors()
    {
        var err = Error.Create("bad", "BAD", ErrorKind.Unexpected);
        var r = Result<int>.Failure(err, "failed");
        Assert.AreEqual(ResultStatus.Failure, r.Status);
        Assert.IsTrue(r.IsFailure(out var errors));
        Assert.IsNotNull(errors);
        Assert.HasCount(1, errors);
        Assert.AreEqual("BAD", errors[0].Code);
        Assert.IsFalse(r.IsSuccess(out var value));
        Assert.AreEqual(0, value);
        Assert.AreEqual("failed", r.Message);
    }

    [TestMethod]
    public void Map_Should_Transform_Value_When_Success()
    {
        var r = Result<int>.Success(10);
        var mapped = r.Map(x => x.ToString());
        Assert.AreEqual(ResultStatus.Success, mapped.Status);
        Assert.IsTrue(mapped.IsSuccess(out var str));
        Assert.AreEqual("10", str);
    }

    [TestMethod]
    public void Map_Should_Preserve_Failure()
    {
        var r = Result<int>.Failure(Error.Unexpected("boom"));
        var mapped = r.Map(x => x.ToString());
        Assert.AreEqual(ResultStatus.Failure, mapped.Status);
        Assert.IsTrue(mapped.IsFailure(out var errors));
        Assert.HasCount(1, errors!);
        Assert.AreEqual("boom", errors![0].ErrorMessage);
    }

    [TestMethod]
    public void Bind_Should_Chain_On_Success()
    {
        var r = Result<int>.Success(5);
        var bound = r.Bind(x => Result<string>.Success((x * 2).ToString()));
        Assert.AreEqual(ResultStatus.Success, bound.Status);
        Assert.IsTrue(bound.IsSuccess(out var s));
        Assert.AreEqual("10", s);
    }

    [TestMethod]
    public void Bind_Should_Skip_On_Failure()
    {
        var r = Result<int>.Failure(Error.Create("fail"));
        var bound = r.Bind(_ => Result<string>.Success("should not happen"));
        Assert.AreEqual(ResultStatus.Failure, bound.Status);
        Assert.IsTrue(bound.IsFailure(out var errors));
        Assert.AreEqual("fail", errors![0].ErrorMessage);
    }

    [TestMethod]
    public void Match_Should_Invoke_Success_Handler()
    {
        var r = Result<int>.Success(7);
        var output = r.Match(v => v * 2, _ => -1);
        Assert.AreEqual(14, output);
    }

    [TestMethod]
    public void Match_Should_Invoke_Failure_Handler()
    {
        var r = Result<int>.Failure(Error.Create("oops"));
        var output = r.Match(v => v * 2, errs => errs.Count);
        Assert.AreEqual(1, output);
    }

    [TestMethod]
    public void Metadata_Should_Be_Preserved_On_Map()
    {
        var meta = new Dictionary<string, object?> { ["k"] = 123 };
        var r = Result<int>.Success(2, metadata: meta);
        var mapped = r.Map(x => x * 3);
        Assert.AreEqual(123, mapped.Metadata["k"]);
    }

    [TestMethod]
    public void Result_NonGeneric_Success()
    {
        var r = Result.Success("done");
        Assert.AreEqual(ResultStatus.Success, r.Status);
        Assert.IsTrue(r.IsSuccess);
        Assert.IsFalse(r.IsFailure);
        Assert.AreEqual("done", r.Message);
    }

    [TestMethod]
    public void Result_NonGeneric_Failure()
    {
        var err = Error.Create("x");
        var r = Result.Failure(err);
        Assert.AreEqual(ResultStatus.Failure, r.Status);
        Assert.IsFalse(r.IsSuccess);
        Assert.IsTrue(r.IsFailure);
        Assert.HasCount(1, ((IResult)r).Errors);
    }

    [TestMethod]
    public void Generic_With_Typed_Error_Success_And_FirstError_Throws()
    {
        var r = Result<string, Error>.Success("abc");
        Assert.AreEqual(ResultStatus.Success, r.Status);
        TestUtils.Expect<InvalidOperationException>(() => { _ = r.FirstError; });
    }

    [TestMethod]
    public void Generic_With_Typed_Error_Failure_FirstError_Returns()
    {
        var err = Error.Create("broken", "E1", ErrorKind.Default);
        var r = Result<string, Error>.Failure(err);
        Assert.AreEqual(ResultStatus.Failure, r.Status);
        Assert.AreEqual(err, r.FirstError);
    }

    [TestMethod]
    public void Deconstruct_Success()
    {
        var r = Result<int, Error>.Success(9);
        var (isSuccess, value, errors) = r;
        Assert.IsTrue(isSuccess);
        Assert.AreEqual(9, value);
        Assert.IsNull(errors);
    }

    [TestMethod]
    public void Deconstruct_Failure()
    {
        var err = Error.Create("bad");
        var r = Result<int, Error>.Failure(err);
        var (isSuccess, value, errors) = r;
        Assert.IsFalse(isSuccess);
        Assert.AreEqual(0, value);
        Assert.IsNotNull(errors);
        Assert.HasCount(1, errors);
    }
}