namespace TodoTasks.Domain.Interfaces;

/// <summary>
/// Coordinates persistence across repositories, committing all staged changes
/// as a single atomic transaction.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets the repository for <see cref="Entities.TodoTask"/> aggregates.
    /// </summary>
    ITodoTaskRepository TodoTasks { get; }

    /// <summary>
    /// Persists all staged changes to the data store as one transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The number of state entries written to the data store.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
