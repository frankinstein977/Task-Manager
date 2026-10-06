using TodoTasks.Domain.Common;
using TodoTasks.Domain.Enums;

namespace TodoTasks.Domain.Entities;

/// <summary>
/// Represents a daily technical task tracked by the engineering team.
/// </summary>
public class TodoTask : BaseEntity
{
    private const int MaxTitleLength = 200;

    /// <summary>
    /// Gets or sets the short descriptive title of the task.
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the detailed description of the work to be performed.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets or sets the current lifecycle status of the task.
    /// </summary>
    public TodoTaskStatus Status { get; private set; }

    /// <summary>
    /// Gets or sets the urgency level of the task.
    /// </summary>
    public TaskPriority Priority { get; private set; }

    /// <summary>
    /// Gets or sets the date the task is scheduled for.
    /// </summary>
    public DateOnly ScheduledDate { get; private set; }

    /// <summary>
    /// Gets or sets the name of the engineer responsible for the task.
    /// </summary>
    public string? Assignee { get; private set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the task was completed, if applicable.
    /// </summary>
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoTask"/> class.
    /// </summary>
    /// <param name="title">The short descriptive title of the task.</param>
    /// <param name="scheduledDate">The date the task is scheduled for.</param>
    /// <param name="priority">The urgency level of the task.</param>
    /// <param name="description">An optional detailed description.</param>
    /// <param name="assignee">An optional assignee name.</param>
    /// <exception cref="ArgumentException">Thrown when the title is null, empty, or exceeds the maximum length.</exception>
    public TodoTask(
        string title,
        DateOnly scheduledDate,
        TaskPriority priority,
        string? description = null,
        string? assignee = null)
    {
        SetTitle(title);
        ScheduledDate = scheduledDate;
        Priority = priority;
        Description = description?.Trim();
        Assignee = assignee?.Trim();
        Status = TodoTaskStatus.Pending;
    }

    /// <summary>
    /// Private parameterless constructor required by EF Core for materialization.
    /// </summary>
    private TodoTask()
    {
    }

    /// <summary>
    /// Updates the mutable details of the task.
    /// </summary>
    /// <param name="title">The new title.</param>
    /// <param name="description">The new description.</param>
    /// <param name="priority">The new priority.</param>
    /// <param name="scheduledDate">The new scheduled date.</param>
    /// <param name="assignee">The new assignee.</param>
    /// <exception cref="InvalidOperationException">Thrown when the task has already been completed or cancelled.</exception>
    public void UpdateDetails(
        string title,
        string? description,
        TaskPriority priority,
        DateOnly scheduledDate,
        string? assignee)
    {
        EnsureEditable();
        SetTitle(title);
        Description = description?.Trim();
        Priority = priority;
        ScheduledDate = scheduledDate;
        Assignee = assignee?.Trim();
        Touch();
    }

    /// <summary>
    /// Transitions the task to a new status, enforcing valid state transitions.
    /// </summary>
    /// <param name="newStatus">The target status.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the transition is not allowed from the current status.
    /// </exception>
    public void ChangeStatus(TodoTaskStatus newStatus)
    {
        if (Status == TodoTaskStatus.Completed || Status == TodoTaskStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Cannot change the status of a task that is already '{Status}'.");
        }

        Status = newStatus;
        CompletedAtUtc = newStatus == TodoTaskStatus.Completed ? DateTime.UtcNow : null;
        Touch();
    }

    /// <summary>
    /// Determines whether the task is overdue relative to the supplied date.
    /// </summary>
    /// <param name="today">The current date to compare against.</param>
    /// <returns><c>true</c> when the scheduled date has passed and the task is not completed or cancelled.</returns>
    public bool IsOverdue(DateOnly today)
    {
        return ScheduledDate < today
            && Status != TodoTaskStatus.Completed
            && Status != TodoTaskStatus.Cancelled;
    }

    private void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("The task title cannot be null or empty.", nameof(title));
        }

        string trimmedTitle = title.Trim();
        if (trimmedTitle.Length > MaxTitleLength)
        {
            throw new ArgumentException(
                $"The task title cannot exceed {MaxTitleLength} characters.", nameof(title));
        }

        Title = trimmedTitle;
    }

    private void EnsureEditable()
    {
        if (Status == TodoTaskStatus.Completed || Status == TodoTaskStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"A task with status '{Status}' can no longer be edited.");
        }
    }

    private void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
