namespace BigOX.Domain;

/// <summary>
///     Interface to mark a repository class. The interface is empty because it is not meant to define
///     operations for repositories as those are not known upfront.
/// </summary>
/// <remarks>
///     1. Refrain from implementing generic repositories. Repository methods should be specific and indicating clear
///     intent.
///     2. The interface is merely introduced to help with dependency injection scenarios and as an output for the unit of
///     work pattern.
/// </remarks>
public interface IRepository;