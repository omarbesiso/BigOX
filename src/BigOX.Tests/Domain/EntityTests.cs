using BigOX.Domain;

namespace BigOX.Tests.Domain;

[TestClass]
public sealed class EntityTests
{
    [TestMethod]
    public void Equals_SameReference_ReturnsTrue()
    {
        var id = Guid.Empty; // transient is fine because ReferenceEquals short-circuits
        var a = new UserEntity(id);
        var same = a;

        Assert.IsTrue(a.Equals(same));
        Assert.IsTrue(((IEntity<Guid>)a).Equals(same));
        Assert.IsTrue(a.Equals((object)same));
        Assert.IsTrue(a == same);
        Assert.IsFalse(a != same);
    }

    [TestMethod]
    public void Equals_SameId_SameType_DistinctInstances_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var a = new UserEntity(id);
        var b = new UserEntity(id);

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(((IEntity<Guid>)a).Equals(b));
        Assert.IsTrue(a.Equals((object)b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    [TestMethod]
    public void NotEquals_DifferentId_SameType_ReturnsFalse()
    {
        var a = new UserEntity(Guid.NewGuid());
        var b = new UserEntity(Guid.NewGuid());

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(((IEntity<Guid>)a).Equals(b));
        Assert.IsFalse(a.Equals((object)b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void NotEquals_SameId_DifferentTypes_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var a = new UserEntity(id);
        var b = new OrderEntity(id);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(((IEntity<Guid>)a).Equals(b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void NotEquals_TwoTransients_ReturnsFalse()
    {
        var a = new UserEntity(Guid.Empty);
        var b = new UserEntity(Guid.Empty);

        Assert.IsFalse(a.Equals(b));
        Assert.IsFalse(((IEntity<Guid>)a).Equals(b));
        Assert.IsFalse(a.Equals((object)b));
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equals_IEntity_AlienImplementation_ReturnsFalse()
    {
        var id = Guid.NewGuid();
        var entity = new UserEntity(id);
        IEntity<Guid> alien = new AlienEntity(id);

        Assert.IsFalse(entity.Equals(alien));
    }

    [TestMethod]
    public void Operator_Equals_HandlesNulls()
    {
        UserEntity? left = null;
        UserEntity? right = null;
        var value = new UserEntity(Guid.NewGuid());

        Assert.IsTrue(left == right);
        Assert.IsFalse(left != right);

        Assert.IsFalse(left == value);
        Assert.IsTrue(left != value);

        Assert.IsFalse(value == right);
        Assert.IsTrue(value != right);
    }

    [TestMethod]
    public void GetHashCode_EqualEntities_HaveSameHashCode()
    {
        var id = Guid.NewGuid();
        var a = new UserEntity(id);
        var b = new UserEntity(id);

        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_IsStableAcrossCalls()
    {
        var a = new UserEntity(Guid.NewGuid());
        var h1 = a.GetHashCode();
        var h2 = a.GetHashCode();
        Assert.AreEqual(h1, h2);
    }

    private sealed class UserEntity : Entity<Guid>
    {
        public UserEntity(Guid id)
        {
            Id = id;
        }
    }

    private sealed class OrderEntity : Entity<Guid>
    {
        public OrderEntity(Guid id)
        {
            Id = id;
        }
    }

    private sealed class AlienEntity(Guid id) : IEntity<Guid>
    {
        public Guid Id { get; } = id;

        // Not actually used by the tests beyond existence; return false to keep trivial.
        public bool Equals(IEntity<Guid>? other)
        {
            return false;
        }
    }
}