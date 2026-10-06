using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;

namespace TodoTasks.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fluent API configuration for the <see cref="TodoTask"/> entity mapping.
/// </summary>
public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<TodoTask> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("todo_tasks");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.ScheduledDate)
            .HasColumnName("scheduled_date")
            .IsRequired();

        builder.Property(t => t.Assignee)
            .HasColumnName("assignee")
            .HasMaxLength(100);

        builder.Property(t => t.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        builder.Property(t => t.UpdatedAtUtc)
            .HasColumnName("updated_at_utc");

        builder.Property(t => t.CompletedAtUtc)
            .HasColumnName("completed_at_utc");

        builder.HasIndex(t => t.ScheduledDate)
            .HasDatabaseName("ix_todo_tasks_scheduled_date");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_todo_tasks_status");
    }
}
