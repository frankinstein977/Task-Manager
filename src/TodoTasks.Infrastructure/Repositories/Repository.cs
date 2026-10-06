using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TodoTasks.Domain.Common;
using TodoTasks.Domain.Interfaces;
using TodoTasks.Infrastructure.Persistence;

namespace TodoTasks.Infrastructure.Repositories;

/// <summary>
/// Generic Entity Framework Core implementation of the repository pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this repository.</typeparam>
public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly TodoTasksDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity}"/> class.
    /// </summary>
    /// <param name="dbContext">The EF Core database context.</param>
    public Repository(TodoTasksDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Gets the underlying queryable set for the entity type.
    /// </summary>
    protected IQueryable<TEntity> Entities => _dbContext.Set<TEntity>().AsQueryable();

    /// <inheritdoc />
    public async Task<TEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Set<TEntity>()
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? filter = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = Entities;
        if (filter is not null)
        {
            query = query.Where(filter);
        }

        return await query.AsNoTracking().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await Entities.AnyAsync(predicate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbContext.Set<TEntity>().Update(entity);
    }

    /// <inheritdoc />
    public void Remove(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbContext.Set<TEntity>().Remove(entity);
    }
}
