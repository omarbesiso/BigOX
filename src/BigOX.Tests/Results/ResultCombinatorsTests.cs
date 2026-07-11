using BigOX.Results;

namespace BigOX.Tests.Results;

[TestClass]
public sealed class ResultCombinatorsTests
{
    private static readonly IReadOnlyDictionary<string, object?> Meta =
        new Dictionary<string, object?> { ["k"] = 42 };

    // ---------------------------------------------------------------- Tap (Result<T>)

    [TestMethod]
    public void Tap_Success_InvokesActionAndReturnsOriginal()
    {
        var captured = 0;
        var r = Result<int>.Success(5, "ok", Meta);

        var same = r.Tap(v => captured = v);

        Assert.AreEqual(5, captured);
        Assert.AreEqual(ResultStatus.Success, same.Status);
        Assert.IsTrue(same.IsSuccess(out var v));
        Assert.AreEqual(5, v);
        Assert.AreEqual("ok", same.Message);
        Assert.AreEqual(42, same.Metadata["k"]);
    }

    [TestMethod]
    public void Tap_Failure_DoesNotInvokeAndReturnsOriginal()
    {
        var invoked = false;
        var r = Result<int>.Failure(Error.Create("bad"));

        var same = r.Tap(_ => invoked = true);

        Assert.IsFalse(invoked);
        Assert.AreEqual(ResultStatus.Failure, same.Status);
    }

    [TestMethod]
    public void Tap_Uninitialized_ThrowsInvalidOperationException()
    {
        var r = default(Result<int>);
        Assert.ThrowsExactly<InvalidOperationException>(() => r.Tap(_ => { }));
    }

    [TestMethod]
    public void Tap_NullAction_ThrowsArgumentNullException()
    {
        var r = Result<int>.Success(1);
        Assert.ThrowsExactly<ArgumentNullException>(() => r.Tap(null!));
    }

    [TestMethod]
    public void Tap_Generic_Success_Invokes()
    {
        var captured = 0;
        var g = Result<int, Error>.Success(9);

        var same = g.Tap(v => captured = v);

        Assert.AreEqual(9, captured);
        Assert.AreEqual(ResultStatus.Success, same.Status);
    }

    // ------------------------------------------------------------ TapError (Result<T>)

    [TestMethod]
    public void TapError_Failure_InvokesWithErrorsAndReturnsOriginal()
    {
        IReadOnlyList<Error>? captured = null;
        var r = Result<int>.Failure(Error.Create("e1"), "msg", Meta);

        var same = r.TapError(errs => captured = errs);

        Assert.IsNotNull(captured);
        Assert.HasCount(1, captured!);
        Assert.AreEqual("e1", captured![0].ErrorMessage);
        Assert.AreEqual(ResultStatus.Failure, same.Status);
        Assert.AreEqual("msg", same.Message);
    }

    [TestMethod]
    public void TapError_Success_DoesNotInvoke()
    {
        var invoked = false;
        var r = Result<int>.Success(1);

        var same = r.TapError(_ => invoked = true);

        Assert.IsFalse(invoked);
        Assert.AreEqual(ResultStatus.Success, same.Status);
    }

    [TestMethod]
    public void TapError_Uninitialized_ThrowsInvalidOperationException()
    {
        var r = default(Result<int>);
        Assert.ThrowsExactly<InvalidOperationException>(() => r.TapError(_ => { }));
    }

    [TestMethod]
    public void TapError_NullAction_ThrowsArgumentNullException()
    {
        var r = Result<int>.Failure(Error.Create("x"));
        Assert.ThrowsExactly<ArgumentNullException>(() => r.TapError(null!));
    }

    [TestMethod]
    public void TapError_Generic_Failure_InvokesWithErrors()
    {
        IReadOnlyList<Error>? captured = null;
        var g = Result<int, Error>.Failure(Error.Create("e2"));

        g.TapError(errs => captured = errs);

        Assert.IsNotNull(captured);
        Assert.AreEqual("e2", captured![0].ErrorMessage);
    }

    // -------------------------------------------------------------- Ensure (Result<T>)

    [TestMethod]
    public void Ensure_SuccessPredicateTrue_ReturnsOriginal()
    {
        var r = Result<int>.Success(20, "ok", Meta);

        var ensured = r.Ensure(v => v > 10, Error.Create("too small"));

        Assert.AreEqual(ResultStatus.Success, ensured.Status);
        Assert.IsTrue(ensured.IsSuccess(out var v));
        Assert.AreEqual(20, v);
    }

    [TestMethod]
    public void Ensure_SuccessPredicateFalse_ReturnsFailurePreservingMessageAndMetadata()
    {
        var r = Result<int>.Success(5, "orig", Meta);
        var err = Error.Create("too small", "SMALL");

        var ensured = r.Ensure(v => v > 10, err);

        Assert.AreEqual(ResultStatus.Failure, ensured.Status);
        Assert.IsTrue(ensured.IsFailure(out var errs));
        Assert.AreEqual("SMALL", errs![0].Code);
        Assert.AreEqual("orig", ensured.Message);
        Assert.AreEqual(42, ensured.Metadata["k"]);
    }

    [TestMethod]
    public void Ensure_Failure_PassesThrough()
    {
        var r = Result<int>.Failure(Error.Create("original"));

        var ensured = r.Ensure(_ => true, Error.Create("unused"));

        Assert.AreEqual(ResultStatus.Failure, ensured.Status);
        Assert.IsTrue(ensured.IsFailure(out var errs));
        Assert.AreEqual("original", errs![0].ErrorMessage);
    }

    [TestMethod]
    public void Ensure_Uninitialized_ThrowsInvalidOperationException()
    {
        var r = default(Result<int>);
        Assert.ThrowsExactly<InvalidOperationException>(() => r.Ensure(_ => true, Error.Create("x")));
    }

    [TestMethod]
    public void Ensure_NullPredicate_ThrowsArgumentNullException()
    {
        var r = Result<int>.Success(1);
        Assert.ThrowsExactly<ArgumentNullException>(() => r.Ensure(null!, Error.Create("x")));
    }

    [TestMethod]
    public void Ensure_NullError_ThrowsArgumentNullException()
    {
        var r = Result<int>.Success(1);
        Assert.ThrowsExactly<ArgumentNullException>(() => r.Ensure(_ => false, null!));
    }

    [TestMethod]
    public void Ensure_Generic_PredicateFalse_ReturnsFailure()
    {
        var g = Result<int, Error>.Success(1);

        var ensured = g.Ensure(v => v > 10, Error.Create("nope", "NOPE"));

        Assert.AreEqual(ResultStatus.Failure, ensured.Status);
        Assert.IsTrue(ensured.IsFailure(out var errs));
        Assert.AreEqual("NOPE", errs![0].Code);
    }

    // ----------------------------------------------------- MapError (Result<TValue,TError>)

    [TestMethod]
    public void MapError_Failure_TransformsEachErrorToNewType()
    {
        var g = Result<int, Error>.Failure(
            new[] { Error.Create("a"), Error.Create("b") }, "msg", Meta);

        var mapped = g.MapError(e => new CustomError("wrapped:" + e.ErrorMessage));

        Assert.AreEqual(ResultStatus.Failure, mapped.Status);
        Assert.IsTrue(mapped.IsFailure(out var errs));
        Assert.HasCount(2, errs!);
        Assert.AreEqual("wrapped:a", errs![0].ErrorMessage);
        Assert.AreEqual("wrapped:b", errs[1].ErrorMessage);
        Assert.AreEqual("msg", mapped.Message);
        Assert.AreEqual(42, mapped.Metadata["k"]);
    }

    [TestMethod]
    public void MapError_Success_PreservesValueMessageMetadata()
    {
        var g = Result<int, Error>.Success(7, "ok", Meta);

        var mapped = g.MapError(e => new CustomError(e.ErrorMessage));

        Assert.AreEqual(ResultStatus.Success, mapped.Status);
        Assert.IsTrue(mapped.IsSuccess(out var v));
        Assert.AreEqual(7, v);
        Assert.AreEqual("ok", mapped.Message);
        Assert.AreEqual(42, mapped.Metadata["k"]);
    }

    [TestMethod]
    public void MapError_Uninitialized_ThrowsInvalidOperationException()
    {
        var g = default(Result<int, Error>);
        Assert.ThrowsExactly<InvalidOperationException>(() => g.MapError(e => new CustomError(e.ErrorMessage)));
    }

    [TestMethod]
    public void MapError_NullMap_ThrowsArgumentNullException()
    {
        var g = Result<int, Error>.Failure(Error.Create("x"));
        Assert.ThrowsExactly<ArgumentNullException>(() => g.MapError<CustomError>(null!));
    }

    // -------------------------------------------------------------- MapAsync (Result<T>)

    [TestMethod]
    public async Task MapAsync_Success_MapsValuePreservingMessageMetadata()
    {
        var r = Result<int>.Success(3, "ok", Meta);

        var mapped = await r.MapAsync(async v =>
        {
            await Task.Yield();
            return v.ToString();
        });

        Assert.IsTrue(mapped.IsSuccess(out var s));
        Assert.AreEqual("3", s);
        Assert.AreEqual("ok", mapped.Message);
        Assert.AreEqual(42, mapped.Metadata["k"]);
    }

    [TestMethod]
    public async Task MapAsync_Failure_Propagates()
    {
        var r = Result<int>.Failure(Error.Create("bad"));

        var mapped = await r.MapAsync(v => Task.FromResult(v.ToString()));

        Assert.AreEqual(ResultStatus.Failure, mapped.Status);
        Assert.IsTrue(mapped.IsFailure(out var errs));
        Assert.AreEqual("bad", errs![0].ErrorMessage);
    }

    [TestMethod]
    public async Task MapAsync_Uninitialized_ThrowsInvalidOperationException()
    {
        var r = default(Result<int>);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await r.MapAsync(v => Task.FromResult(v)));
    }

    [TestMethod]
    public async Task MapAsync_NullMap_ThrowsArgumentNullException()
    {
        var r = Result<int>.Success(1);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await r.MapAsync<int>(null!));
    }

    [TestMethod]
    public async Task MapAsync_Generic_Success_MapsValue()
    {
        var g = Result<int, Error>.Success(4);

        var mapped = await g.MapAsync(v => Task.FromResult(v * 2));

        Assert.IsTrue(mapped.IsSuccess(out var v));
        Assert.AreEqual(8, v);
    }

    [TestMethod]
    public async Task MapAsync_Generic_Failure_Propagates()
    {
        var g = Result<int, Error>.Failure(Error.Create("bad"));

        var mapped = await g.MapAsync(v => Task.FromResult(v * 2));

        Assert.AreEqual(ResultStatus.Failure, mapped.Status);
        Assert.IsTrue(mapped.IsFailure(out var errs));
        Assert.AreEqual("bad", errs![0].ErrorMessage);
    }

    // ------------------------------------------------------------- BindAsync (Result<T>)

    [TestMethod]
    public async Task BindAsync_Success_ChainsResult()
    {
        var r = Result<int>.Success(5);

        var bound = await r.BindAsync(v => Task.FromResult(Result<string>.Success((v * 2).ToString())));

        Assert.IsTrue(bound.IsSuccess(out var s));
        Assert.AreEqual("10", s);
    }

    [TestMethod]
    public async Task BindAsync_Failure_SkipsContinuation()
    {
        var invoked = false;
        var r = Result<int>.Failure(Error.Create("fail"));

        var bound = await r.BindAsync(v =>
        {
            invoked = true;
            return Task.FromResult(Result<string>.Success(v.ToString()));
        });

        Assert.IsFalse(invoked);
        Assert.AreEqual(ResultStatus.Failure, bound.Status);
        Assert.IsTrue(bound.IsFailure(out var errs));
        Assert.AreEqual("fail", errs![0].ErrorMessage);
    }

    [TestMethod]
    public async Task BindAsync_Uninitialized_ThrowsInvalidOperationException()
    {
        var r = default(Result<int>);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            async () => await r.BindAsync(v => Task.FromResult(Result<int>.Success(v))));
    }

    [TestMethod]
    public async Task BindAsync_NullBind_ThrowsArgumentNullException()
    {
        var r = Result<int>.Success(1);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await r.BindAsync<int>(null!));
    }

    [TestMethod]
    public async Task BindAsync_Generic_Success_ChainsResult()
    {
        var g = Result<int, Error>.Success(5);

        var bound = await g.BindAsync(v => Task.FromResult(Result<string, Error>.Success((v * 2).ToString())));

        Assert.IsTrue(bound.IsSuccess(out var s));
        Assert.AreEqual("10", s);
    }

    [TestMethod]
    public async Task BindAsync_Generic_Failure_Propagates()
    {
        var g = Result<int, Error>.Failure(Error.Create("fail"));

        var bound = await g.BindAsync(v => Task.FromResult(Result<string, Error>.Success(v.ToString())));

        Assert.AreEqual(ResultStatus.Failure, bound.Status);
        Assert.IsTrue(bound.IsFailure(out var errs));
        Assert.AreEqual("fail", errs![0].ErrorMessage);
    }

    private sealed record CustomError(string ErrorMessage) : IError
    {
        public string Code => "CUSTOM";

        public Exception? Exception => null;

        public ErrorKind Kind => ErrorKind.Default;

        public IReadOnlyDictionary<string, object?> Metadata { get; } = new Dictionary<string, object?>();
    }
}
