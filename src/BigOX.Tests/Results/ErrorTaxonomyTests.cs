using BigOX.Results;

namespace BigOX.Tests.Results;

[TestClass]
public sealed class ErrorTaxonomyTests
{
    [TestMethod]
    public void NewErrorKinds_HaveExpectedValues()
    {
        Assert.AreEqual("Validation", ErrorKind.Validation.Value);
        Assert.AreEqual("NotFound", ErrorKind.NotFound.Value);
        Assert.AreEqual("Conflict", ErrorKind.Conflict.Value);
        Assert.AreEqual("Unauthorized", ErrorKind.Unauthorized.Value);
        Assert.AreEqual("Forbidden", ErrorKind.Forbidden.Value);
    }

    [TestMethod]
    public void Validation_ProducesValidationKindWithCodeFallback()
    {
        var e = Error.Validation("invalid");
        Assert.AreEqual(ErrorKind.Validation, e.Kind);
        Assert.AreEqual("Validation", e.Code);
        Assert.AreEqual("invalid", e.ErrorMessage);
    }

    [TestMethod]
    public void NotFound_ProducesNotFoundKindWithCodeFallback()
    {
        var e = Error.NotFound("missing");
        Assert.AreEqual(ErrorKind.NotFound, e.Kind);
        Assert.AreEqual("NotFound", e.Code);
    }

    [TestMethod]
    public void Conflict_ProducesConflictKindWithCodeFallback()
    {
        var e = Error.Conflict("clash");
        Assert.AreEqual(ErrorKind.Conflict, e.Kind);
        Assert.AreEqual("Conflict", e.Code);
    }

    [TestMethod]
    public void Unauthorized_ProducesUnauthorizedKindWithCodeFallback()
    {
        var e = Error.Unauthorized("who?");
        Assert.AreEqual(ErrorKind.Unauthorized, e.Kind);
        Assert.AreEqual("Unauthorized", e.Code);
    }

    [TestMethod]
    public void Forbidden_ProducesForbiddenKindWithCodeFallback()
    {
        var e = Error.Forbidden("nope");
        Assert.AreEqual(ErrorKind.Forbidden, e.Kind);
        Assert.AreEqual("Forbidden", e.Code);
    }

    [TestMethod]
    public void Factory_HonorsExplicitCode()
    {
        var e = Error.Conflict("dup", "DUP_KEY");
        Assert.AreEqual("DUP_KEY", e.Code);
        Assert.AreEqual(ErrorKind.Conflict, e.Kind);
    }

    [TestMethod]
    public void Factory_WhitespaceCode_FallsBackToKindValue()
    {
        var e = Error.NotFound("missing", "   ");
        Assert.AreEqual("NotFound", e.Code);
    }

    [TestMethod]
    public void Factory_FlowsMetadataAndException()
    {
        var ex = new InvalidOperationException("boom");
        var meta = new Dictionary<string, object?> { ["k"] = 1 };

        var e = Error.Unauthorized("no", exception: ex, metadata: meta);

        Assert.AreEqual(ErrorKind.Unauthorized, e.Kind);
        Assert.AreEqual(1, e.Metadata["k"]);
        Assert.AreSame(ex, e.Exception);
    }

    [TestMethod]
    public void Factory_NullMessage_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => Error.Validation(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => Error.NotFound(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => Error.Conflict(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => Error.Unauthorized(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => Error.Forbidden(null!));
    }
}
