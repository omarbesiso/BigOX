namespace BigOX.Domain;

/// <summary>
///     A base entity implementation that provides an equality mechanism by comparing unique identifiers and
///     includes infrastructure for setting properties with interception.
/// </summary>
/// <typeparam name="TId">The type of the unique identifier.</typeparam>
/// <remarks>
///     Initializes a new instance of the <see cref="Entity{TId}" /> class.
/// </remarks>
public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
    where TId : struct, IEquatable<TId>
{
    /// <summary>
    ///     Gets the unique identifier of the entity.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public TId Id { get; protected set; }

    /// <summary>
    ///     Indicates whether the current entity is equal to another entity of the same type.
    /// </summary>
    /// <param name="other">An entity to compare with this entity.</param>
    /// <returns>
    ///     <see langword="true" /> if the current entity is equal to the <paramref name="other" /> parameter; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(IEntity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        // Ensure same runtime type to avoid cross-type equality even when IDs match
        if (other is not Entity<TId> otherEntity || otherEntity.GetType() != GetType())
        {
            return false;
        }

        // Do not consider transient entities (with default Id) as equal
        if (IsDefault(Id) || IsDefault(otherEntity.Id))
        {
            return false;
        }

        return Id.Equals(otherEntity.Id);
    }

    /// <summary>
    ///     Indicates whether the current entity is equal to another entity of the same type.
    /// </summary>
    /// <param name="other">An entity to compare with this entity.</param>
    /// <returns>
    ///     <see langword="true" /> if the current entity is equal to the <paramref name="other" /> parameter; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other.GetType() != GetType())
        {
            return false;
        }

        if (IsDefault(Id) || IsDefault(other.Id))
        {
            return false;
        }

        return Id.Equals(other.Id);
    }

    /// <summary>
    ///     Determines whether the specified <see cref="object" /> is equal to this entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this entity; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other && Equals(other);
    }

    /// <summary>
    ///     Returns a hash code for this entity.
    /// </summary>
    /// <returns>A hash code for this entity, suitable for use in hashing algorithms and data structures like a hash table.</returns>
    // ReSharper disable once NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return HashCode.Combine(Id);
    }

    /// <summary>
    ///     Determines whether two entities are equal.
    /// </summary>
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    ///     Determines whether two entities are not equal.
    /// </summary>
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !(left == right);
    }

    private static bool IsDefault(TId id)
    {
        return EqualityComparer<TId>.Default.Equals(id, default);
    }
}