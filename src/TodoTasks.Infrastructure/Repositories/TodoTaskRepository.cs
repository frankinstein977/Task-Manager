using Microsoft.EntityFrameworkCore;
using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;
using TodoTasks.Domain.Interfaces;
using TodoTasks.Infrastructure.Persistence;

namespace TodoTasks.Infrastructure.Repositories;

/// <summary>
/// Task-specific repository implementation with domain-focused queries.
/// </summary>
public class TodoTaskRepository : Repository<TodoTask>, ITodoTaskRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TodoTaskRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The EF Core database context.</param>
    public TodoTaskRepository(TodoTasksDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TodoTask>> GetByScheduledDateAsync(
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .Where(task => task.ScheduledDate == scheduledDate)
            .OrderByDescending(task => task.Priority)
            .ThenBy(task => task.Title)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TodoTask>> GetByStatusAsync(
        TodoTaskStatus status,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .Where(task => task.Status == status)
            .OrderBy(task => task.ScheduledDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TodoTask>> GetOverdueTasksAsync(
        DateOnly today,
        CancellationToken cancellationToken = default)
    {
        return await Entities
            .Where(task => task.ScheduledDate < today
                && task.Status != TodoTaskStatus.Completed
                && task.Status != TodoTaskStatus.Cancelled)
            .OrderBy(task => task.ScheduledDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
