using TodoTasks.Domain.Interfaces;
using TodoTasks.Infrastructure.Persistence;

namespace TodoTasks.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation that shares a single <see cref="TodoTasksDbContext"/>
/// across repositories, guaranteeing that all staged changes commit atomically.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TodoTasksDbContext _dbContext;
    private ITodoTaskRepository? _todoTaskRepository;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
    /// </summary>
    /// <param name="dbContext">The EF Core database context shared by all repositories.</param>
    public UnitOfWork(TodoTasksDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc />
    public ITodoTaskRepository TodoTasks =>
        _todoTaskRepository ??= new TodoTaskRepository(_dbContext);

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Releases the underlying database context.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and, optionally, managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _dbContext.Dispose();
        }

        _disposed = true;
    }
}
