using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class DoubleExtensionsTests
{
    [TestMethod]
    public void ToDecimal_NullableDouble_WithValue_ConvertsToDecimalPreservingMagnitude()
    {
        double? value = 1234.56;
        var d = value.ToDecimal();
        Assert.IsNotNull(d);
        Assert.AreEqual(1234.56m, d!.Value);
    }

    [TestMethod]
    public void ToDecimal_NullableDouble_Null_ReturnsNull()
    {
        double? value = null;
        var d = value.ToDecimal();
        Assert.IsNull(d);
    }

    [TestMethod]
    public void ToDecimal_NullableDouble_HighPrecisionDouble_RoundsAsPerDecimalConversion()
    {
        // Double cannot represent certain fractional values exactly; ensure conversion uses current double value
        double? value = 0.1; // binary approximation ~0.10000000000000000555...
        var d = value.ToDecimal();
        Assert.IsNotNull(d);
        // Decimal conversion of double 0.1 becomes 0.1000000000000000055511151231m typically truncated to available precision
        // Assert by converting back to double to avoid false negatives due to representation nuances
        Assert.AreEqual((decimal)value.Value, d!.Value);
    }

    [TestMethod]
    public void ToDecimal_NullableDouble_MaxValue_ConvertsWithoutOverflowIfWithinDecimalRange()
    {
        // Use a large but decimal-compatible double
        double? value = 7922816251426433000d; // less than decimal.MaxValue (~7.922816251426433759353...E28)
        var d = value.ToDecimal();
        Assert.IsNotNull(d);
        Assert.AreEqual((decimal)value.Value, d!.Value);
    }
}