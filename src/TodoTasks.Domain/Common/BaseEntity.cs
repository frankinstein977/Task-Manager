namespace TodoTasks.Domain.Common;

/// <summary>
/// Base class for all domain entities, providing identity and audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was last modified.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}
