namespace TodoTasks.Domain.Enums;

/// <summary>
/// Represents the urgency level of a technical task.
/// </summary>
public enum TaskPriority
{
    /// <summary>Low priority; can be handled when time permits.</summary>
    Low = 0,

    /// <summary>Medium priority; part of the regular daily workload.</summary>
    Medium = 1,

    /// <summary>High priority; should be addressed as soon as possible.</summary>
    High = 2,

    /// <summary>Critical priority; requires immediate attention.</summary>
    Critical = 3
}
