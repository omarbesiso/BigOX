using BigOX.Validation;

namespace BigOX.Tests.Validation;

[TestClass]
public class PropertyGuardTests
{
    [TestMethod]
    public void NotNullOrWhiteSpace_Works_In_PropertySetter()
    {
        var s = new Sample();
        s.Name = "Alice";
        Assert.AreEqual("Alice", s.Name);
        TestUtils.Expect<ArgumentNullException>(() => s.Name = null!);
        TestUtils.Expect<ArgumentException>(() => s.Name = "   ");
    }

    [TestMethod]
    public void NotEmpty_Guid_Works_In_PropertySetter()
    {
        var s = new Sample();
        var id = Guid.NewGuid();
        s.Id = id;
        Assert.AreEqual(id, s.Id);
        TestUtils.Expect<ArgumentException>(() => s.Id = Guid.Empty);
    }

    [TestMethod]
    public void WithinRange_Works_In_PropertySetter()
    {
        var s = new Sample();
        s.Age = 30;
        Assert.AreEqual(30, s.Age);
        TestUtils.Expect<ArgumentOutOfRangeException>(() => s.Age = -1);
        TestUtils.Expect<ArgumentOutOfRangeException>(() => s.Age = 200);
    }

    [TestMethod]
    public void EmailAddress_Works_In_PropertySetter()
    {
        var s = new Sample();
        s.Email = null;
        s.Email = "user@example.com";
        TestUtils.Expect<ArgumentException>(() => s.Email = "invalid@");
    }

    [TestMethod]
    public void Requires_Delegates_To_Guard()
    {
        var value = 5;
        var ok = PropertyGuard.Requires(value, v => v > 0);
        Assert.AreEqual(5, ok);
        TestUtils.Expect<ArgumentException>(() => PropertyGuard.Requires(value, v => v < 0));
        TestUtils.Expect<ArgumentNullException>(() => PropertyGuard.Requires(value, null!));
    }

    [TestMethod]
    public void MinMaxExactLength_Delegates_To_Guard()
    {
        Assert.AreEqual("abc", PropertyGuard.MinLength("abc", 2));
        Assert.AreEqual("abc", PropertyGuard.MaxLength("abc", 3));
        Assert.AreEqual("abc", PropertyGuard.ExactLength("abc", 3));
        TestUtils.Expect<ArgumentException>(() => PropertyGuard.MinLength("a", 2));
        TestUtils.Expect<ArgumentException>(() => PropertyGuard.MaxLength("abcd", 3));
        TestUtils.Expect<ArgumentException>(() => PropertyGuard.ExactLength("ab", 3));
    }

    [TestMethod]
    public void RangeAndDefault_Delegates_To_Guard()
    {
        Assert.AreEqual(5, PropertyGuard.Minimum(5, 0));
        Assert.AreEqual(5, PropertyGuard.Maximum(5, 10));
        Assert.AreEqual(5, PropertyGuard.WithinRange(5, 0, 10));
        TestUtils.Expect<ArgumentOutOfRangeException>(() => PropertyGuard.Minimum(-1, 0));
        TestUtils.Expect<ArgumentOutOfRangeException>(() => PropertyGuard.Maximum(11, 10));
        TestUtils.Expect<ArgumentOutOfRangeException>(() => PropertyGuard.WithinRange(11, 0, 10));
        TestUtils.Expect<ArgumentException>(() => PropertyGuard.NotDefault(default(Guid)));
    }

    [TestMethod]
    public void PropertyGuard_LengthWithinRange_Captures_PropertyName_As_ParamName()
    {
        var s = new Sample();
        // Force failure with a custom message and check ParamName equals property name
        var ex = TestUtils.Expect<ArgumentException>(() => s.LengthBounded = "too long for range");
        StringAssert.Contains(ex.ParamName, nameof(Sample.LengthBounded));
    }

    [TestMethod]
    public void PropertyGuard_Url_Captures_PropertyName_As_ParamName()
    {
        var s = new Sample();
        var ex = TestUtils.Expect<ArgumentException>(() => s.Website = "notaurl");
        StringAssert.Contains(ex.ParamName, nameof(Sample.Website));
    }

    private class Sample
    {
        private int _age;
        private string? _email;
        private Guid _id;
        private string? _lengthBounded;
        private string _name = string.Empty;
        private string? _website;

        public string Name
        {
            get => _name;
            set => _name = PropertyGuard.NotNullOrWhiteSpace(value);
        }

        public Guid Id
        {
            get => _id;
            set => _id = PropertyGuard.NotEmpty(value);
        }

        public int Age
        {
            get => _age;
            set => _age = PropertyGuard.WithinRange(value, 0, 150);
        }

        public string? Email
        {
            get => _email;
            set => _email = PropertyGuard.EmailAddress(value);
        }

        public string? LengthBounded
        {
            get => _lengthBounded;
            set => _lengthBounded = PropertyGuard.LengthWithinRange(value, 1, 5);
        }

        public string? Website
        {
            get => _website;
            set => _website = PropertyGuard.Url(value);
        }
    }
}