using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

[TestClass]
public sealed class DecimalExtensionsTests
{
    [TestMethod]
    public void ToDouble_NullableDecimal_WithValue_ConvertsToDouble()
    {
        decimal? value = 123.45m;
        var d = value.ToDouble();
        Assert.IsNotNull(d);
        Assert.AreEqual(123.45, d!.Value, 1e-9);
    }

    [TestMethod]
    public void ToDouble_NullableDecimal_Null_ReturnsNull()
    {
        decimal? value = null;
        var d = value.ToDouble();
        Assert.IsNull(d);
    }

    [TestMethod]
    public void ToCurrencyString_EnUs_FormatsWithDollarSymbol()
    {
        var amount = 1234.56m;
        var s = amount.ToCurrencyString();
        Assert.AreEqual("$1,234.56", s);
    }

    [TestMethod]
    public void ToCurrencyString_FrFr_ContainsEuroAndComma()
    {
        var amount = 1234.56m;
        var s = amount.ToCurrencyString("fr-FR");
        // Locale-specific spacing and NBSP can vary; verify key parts instead of exact string
        Assert.Contains('€', s);
        Assert.Contains(',', s);
        Assert.EndsWith("€", s);
    }

    [TestMethod]
    public void ToPercentageString_EnUs_RespectsDecimalPlaces()
    {
        var value = 0.12345m;
        var s1 = value.ToPercentageString(1);
        var s2 = value.ToPercentageString();

        // en-US typically formats as "12.3 %" and "12.35 %"; assert prefix/rounding and symbol presence
        Assert.StartsWith("12.3", s1);
        Assert.Contains('%', s1);
        Assert.StartsWith("12.35", s2);
        Assert.Contains('%', s2);
    }

    [TestMethod]
    public void RoundTo_BankersRounding_DefaultToEven()
    {
        Assert.AreEqual(1.2m, 1.25m.RoundTo(1)); // 1.25 -> 1.2 (to even)
        Assert.AreEqual(1.4m, 1.35m.RoundTo(1)); // 1.35 -> 1.4 (to even)
        Assert.AreEqual(-1.2m, (-1.25m).RoundTo(1));
    }

    [TestMethod]
    public void IsWholeNumber_TrueForIntegralValues_FalseOtherwise()
    {
        Assert.IsTrue(2.0m.IsWholeNumber());
        Assert.IsTrue((-3.000m).IsWholeNumber());
        Assert.IsFalse(2.5m.IsWholeNumber());
        Assert.IsFalse((-3.1m).IsWholeNumber());
    }

    [TestMethod]
    public void Abs_ReturnsAbsoluteValue()
    {
        Assert.AreEqual(1.23m, (-1.23m).Abs());
        Assert.AreEqual(5m, 5m.Abs());
    }

    [TestMethod]
    public void ToWords_Zero_ReturnsZero()
    {
        Assert.AreEqual("zero", 0m.ToWords());
    }

    [TestMethod]
    public void ToWords_IntegerWithoutCents_NoCentsAppended()
    {
        Assert.AreEqual("five", 5m.ToWords());
        Assert.AreEqual("one hundred and five", 105m.ToWords());
        Assert.AreEqual("one million", 1_000_000m.ToWords());
    }

    [TestMethod]
    public void ToWords_WithCents_AppendsCentsPart()
    {
        Assert.AreEqual("one and twenty-three cents", 1.23m.ToWords());
        Assert.AreEqual("one thousand two hundred and thirty-four and fifty-six cents", 1234.56m.ToWords());
    }

    [TestMethod]
    public void ToWords_NegativeValues_IncludeMinusAndCents()
    {
        Assert.AreEqual("minus two", (-2m).ToWords());
        Assert.AreEqual("minus one and twenty cents", (-1.20m).ToWords());
    }

    [TestMethod]
    public void ToWords_HyphenatedTens_UsesHyphen()
    {
        Assert.AreEqual("twenty-one", 21m.ToWords());
        Assert.AreEqual("ninety-nine", 99m.ToWords());
    }
}