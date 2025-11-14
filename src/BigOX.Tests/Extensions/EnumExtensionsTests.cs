using System.ComponentModel.DataAnnotations;
using BigOX.Extensions;

namespace BigOX.Tests.Extensions;

public enum SampleEnum
{
    [System.ComponentModel.Description("First Value Description")] [Display(Name = "First Value Display")]
    First = 1,

    // No attributes to test fallback behavior
    Second = 2,

    [System.ComponentModel.Description("Third Value Description")] [Display(Name = "Third Value Display")]
    Third = 3
}

[TestClass]
public sealed class EnumExtensionsTests
{
    [TestMethod]
    public void ToDictionary_ReturnsDescriptionKeysAndNamesValues()
    {
        var dict = EnumExtensions.ToDictionary<SampleEnum>();

        Assert.HasCount(3, dict);
        Assert.AreEqual("First", dict["First Value Description"]);
        Assert.AreEqual("Second", dict["Second"]); // Fallback to name when no Description
        Assert.AreEqual("Third", dict["Third Value Description"]);
    }

    [TestMethod]
    public void GetEnumDescription_WithDescriptionAttribute_ReturnsDescription()
    {
        var description = SampleEnum.First.GetEnumDescription();
        Assert.AreEqual("First Value Description", description);
    }

    [TestMethod]
    public void GetEnumDescription_WithoutDescriptionAttribute_FallbacksToName()
    {
        var description = SampleEnum.Second.GetEnumDescription();
        Assert.AreEqual("Second", description);
    }

    [TestMethod]
    public void GetEnumDisplay_WithDisplayAttribute_ReturnsDisplayName()
    {
        var display = SampleEnum.Third.GetEnumDisplay();
        Assert.AreEqual("Third Value Display", display);
    }

    [TestMethod]
    public void GetEnumDisplay_WithoutDisplayAttribute_ReturnsEmptyString()
    {
        var display = SampleEnum.Second.GetEnumDisplay();
        Assert.AreEqual(string.Empty, display);
    }

    [TestMethod]
    public void ToDictionary_DuplicateDescriptions_ThrowsInvalidOperationException()
    {
        Assert.ThrowsExactly<InvalidOperationException>(EnumExtensions.ToDictionary<DuplicateDescriptionEnum>);
    }

    private enum DuplicateDescriptionEnum
    {
        [System.ComponentModel.Description("Duplicate")]
        A = 1,

        [System.ComponentModel.Description("Duplicate")]
        B = 2
    }
}