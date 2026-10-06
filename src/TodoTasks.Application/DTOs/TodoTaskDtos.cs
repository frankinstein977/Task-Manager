using System.ComponentModel.DataAnnotations;
using TodoTasks.Domain.Enums;

namespace TodoTasks.Application.DTOs;

/// <summary>
/// Request payload for creating a new technical task.
/// </summary>
/// <param name="Title">The short descriptive title of the task.</param>
/// <param name="Description">An optional detailed description.</param>
/// <param name="Priority">The urgency level of the task.</param>
/// <param name="ScheduledDate">The date the task is scheduled for (YYYY-MM-DD).</param>
/// <param name="Assignee">An optional assignee name.</param>
public record CreateTodoTaskRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: MaxLength(2000)] string? Description,
    [property: Required] TaskPriority Priority,
    [property: Required] DateOnly ScheduledDate,
    [property: MaxLength(100)] string? Assignee);

/// <summary>
/// Request payload for updating an existing technical task.
/// </summary>
/// <param name="Title">The new title.</param>
/// <param name="Description">The new description.</param>
/// <param name="Priority">The new priority.</param>
/// <param name="ScheduledDate">The new scheduled date (YYYY-MM-DD).</param>
/// <param name="Assignee">The new assignee.</param>
public record UpdateTodoTaskRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: MaxLength(2000)] string? Description,
    [property: Required] TaskPriority Priority,
    [property: Required] DateOnly ScheduledDate,
    [property: MaxLength(100)] string? Assignee);

/// <summary>
/// Request payload for transitioning a task to a new status.
/// </summary>
/// <param name="Status">The target status.</param>
public record ChangeTaskStatusRequest([property: Required] TodoTaskStatus Status);

/// <summary>
/// Read model returned to API consumers for a technical task.
/// </summary>
/// <param name="Id">The unique identifier of the task.</param>
/// <param name="Title">The task title.</param>
/// <param name="Description">The task description.</param>
/// <param name="Status">The current lifecycle status.</param>
/// <param name="Priority">The urgency level.</param>
/// <param name="ScheduledDate">The scheduled date.</param>
/// <param name="Assignee">The assignee name.</param>
/// <param name="IsOverdue">Whether the task is overdue as of the response time.</param>
/// <param name="CreatedAtUtc">The creation timestamp in UTC.</param>
/// <param name="UpdatedAtUtc">The last modification timestamp in UTC.</param>
/// <param name="CompletedAtUtc">The completion timestamp in UTC, when applicable.</param>
public record TodoTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    TodoTaskStatus Status,
    TaskPriority Priority,
    DateOnly ScheduledDate,
    string? Assignee,
    bool IsOverdue,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? CompletedAtUtc);
