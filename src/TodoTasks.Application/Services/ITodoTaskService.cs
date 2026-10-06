using TodoTasks.Application.DTOs;
using TodoTasks.Domain.Enums;

namespace TodoTasks.Application.Services;

/// <summary>
/// Defines the application-level use cases for managing daily technical tasks.
/// </summary>
public interface ITodoTaskService
{
    /// <summary>
    /// Retrieves a single task by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The task read model.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the task does not exist.</exception>
    Task<TodoTaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all tasks, optionally filtered by scheduled date and/or status.
    /// </summary>
    /// <param name="scheduledDate">An optional scheduled date filter.</param>
    /// <param name="status">An optional status filter.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of task read models.</returns>
    Task<IReadOnlyList<TodoTaskResponse>> GetAllAsync(
        DateOnly? scheduledDate = null,
        TodoTaskStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all overdue, unfinished tasks.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of overdue task read models.</returns>
    Task<IReadOnlyList<TodoTaskResponse>> GetOverdueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new technical task.
    /// </summary>
    /// <param name="request">The creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The created task read model.</returns>
    Task<TodoTaskResponse> CreateAsync(
        CreateTodoTaskRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing technical task.
    /// </summary>
    /// <param name="id">The identifier of the task to update.</param>
    /// <param name="request">The update payload.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated task read model.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the task does not exist.</exception>
    Task<TodoTaskResponse> UpdateAsync(
        Guid id,
        UpdateTodoTaskRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transitions a task to a new status.
    /// </summary>
    /// <param name="id">The identifier of the task.</param>
    /// <param name="request">The status change payload.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated task read model.</returns>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the task does not exist.</exception>
    Task<TodoTaskResponse> ChangeStatusAsync(
        Guid id,
        ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task permanently.
    /// </summary>
    /// <param name="id">The identifier of the task to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <exception cref="Exceptions.NotFoundException">Thrown when the task does not exist.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
