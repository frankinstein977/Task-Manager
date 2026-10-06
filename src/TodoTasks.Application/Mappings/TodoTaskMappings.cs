using TodoTasks.Application.DTOs;
using TodoTasks.Domain.Entities;

namespace TodoTasks.Application.Mappings;

/// <summary>
/// Provides mapping helpers between <see cref="TodoTask"/> entities and DTOs.
/// </summary>
public static class TodoTaskMappings
{
    /// <summary>
    /// Maps a <see cref="TodoTask"/> entity to its API read model.
    /// </summary>
    /// <param name="task">The entity to map.</param>
    /// <returns>The corresponding <see cref="TodoTaskResponse"/>.</returns>
    public static TodoTaskResponse ToResponse(this TodoTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return new TodoTaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.ScheduledDate,
            task.Assignee,
            task.IsOverdue(DateOnly.FromDateTime(DateTime.UtcNow)),
            task.CreatedAtUtc,
            task.UpdatedAtUtc,
            task.CompletedAtUtc);
    }
}
