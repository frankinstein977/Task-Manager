using System.Linq.Expressions;
using TodoTasks.Domain.Common;

namespace TodoTasks.Domain.Interfaces;

/// <summary>
/// Defines a generic repository contract for aggregate persistence operations.
/// </summary>
/// <typeparam name="TEntity">The entity type, which must derive from <see cref="BaseEntity"/>.</typeparam>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The entity when found; otherwise <c>null</c>.</returns>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities matching the optional filter.
    /// </summary>
    /// <param name="filter">An optional predicate used to filter the result set.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of matching entities.</returns>
    Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether any entity satisfies the supplied predicate.
    /// </summary>
    /// <param name="predicate">The predicate to evaluate.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><c>true</c> when at least one entity matches; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages a new entity for insertion into the data store.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages an existing entity for update in the data store.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Stages an entity for removal from the data store.
    /// </summary>
    /// <param name="entity">The entity to remove.</param>
    void Remove(TEntity entity);
}
