using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;

namespace TodoTasks.Domain.Interfaces;

/// <summary>
/// Defines task-specific persistence operations beyond the generic repository contract.
/// </summary>
public interface ITodoTaskRepository : IRepository<TodoTask>
{
    /// <summary>
    /// Retrieves all tasks scheduled for a specific date.
    /// </summary>
    /// <param name="scheduledDate">The scheduled date to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of tasks scheduled for the given date.</returns>
    Task<IReadOnlyList<TodoTask>> GetByScheduledDateAsync(
        DateOnly scheduledDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tasks currently assigned the supplied status.
    /// </summary>
    /// <param name="status">The status to filter by.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of tasks in the given status.</returns>
    Task<IReadOnlyList<TodoTask>> GetByStatusAsync(
        TodoTaskStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all overdue tasks relative to the supplied date.
    /// </summary>
    /// <param name="today">The current date used as the overdue threshold.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of overdue, unfinished tasks.</returns>
    Task<IReadOnlyList<TodoTask>> GetOverdueTasksAsync(
        DateOnly today,
        CancellationToken cancellationToken = default);
}
