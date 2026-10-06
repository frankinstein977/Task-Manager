namespace TodoTasks.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a technical task.
/// </summary>
public enum TodoTaskStatus
{
    /// <summary>The task has been created but work has not started.</summary>
    Pending = 0,

    /// <summary>Work on the task is currently in progress.</summary>
    InProgress = 1,

    /// <summary>The task has been completed.</summary>
    Completed = 2,

    /// <summary>The task has been cancelled and will not be pursued.</summary>
    Cancelled = 3
}
