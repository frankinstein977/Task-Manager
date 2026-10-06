using Microsoft.EntityFrameworkCore;
using TodoTasks.Domain.Entities;

namespace TodoTasks.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core database context backed by PostgreSQL.
/// </summary>
public class TodoTasksDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodoTasksDbContext"/> class.
    /// </summary>
    /// <param name="options">The configured context options.</param>
    public TodoTasksDbContext(DbContextOptions<TodoTasksDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> for technical tasks.
    /// </summary>
    public DbSet<TodoTask> TodoTasks => Set<TodoTask>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TodoTasksDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
