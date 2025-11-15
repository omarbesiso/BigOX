namespace BigOX.Domain;

/// <summary>
///     Interface defining a unit of work implementation, which maintains a list of objects affected by a business
///     transaction and coordinates the writing out of changes and the resolution of concurrency problems.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Commits the changes that occurred within the scope of the unit of work.
    /// </summary>
    /// <remarks>
    ///     This method saves all the changes made within the current unit of work context to the database. It is intended for
    ///     synchronous operations.
    /// </remarks>
    void Commit();

    /// <summary>
    ///     Asynchronously commits the changes that occurred within the scope of the unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation. </param>
    /// <remarks>
    ///     This method saves all the changes made within the current unit of work context to the database. It is intended for
    ///     asynchronous operations.
    /// </remarks>
    /// <returns>A task that represents the asynchronous commit operation.</returns>
    Task CommitAsync(CancellationToken cancellationToken = default);
}