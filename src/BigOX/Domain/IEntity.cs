namespace BigOX.Domain;

/// <summary>
///     Contract for defining an entity.
/// </summary>
/// <remarks>
///     In domain driven design the only thing that can be generically defined in an entity's structure is a unique
///     identifier that is used
///     identify the entity. From a behavioral perspective the only entity behavior we can generically identify is the
///     equality of an entity. By default, the implementation of equality should be based on comparing the unique
///     identifier
///     of an instance of the entity with another instance of that same entity or an instance of one of its child
///     implementations.
/// </remarks>
/// <typeparam name="TId">The type of the unique identifier.</typeparam>
public interface IEntity<TId> : IEquatable<IEntity<TId>>
    where TId : struct
{
    /// <summary>
    ///     Gets the unique identifier of the entity.
    /// </summary>
    TId Id { get; }
}