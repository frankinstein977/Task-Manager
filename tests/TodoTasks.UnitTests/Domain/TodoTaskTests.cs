using FluentAssertions;
using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;
using Xunit;

namespace TodoTasks.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="TodoTask"/> domain entity.
/// </summary>
public class TodoTaskTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesPendingTask()
    {
        // Arrange
        DateOnly scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        var task = new TodoTask(
            "Fix CI pipeline", scheduledDate, TaskPriority.High, "Investigate flaky tests", "alice");

        // Assert
        task.Title.Should().Be("Fix CI pipeline");
        task.Description.Should().Be("Investigate flaky tests");
        task.Assignee.Should().Be("alice");
        task.Priority.Should().Be(TaskPriority.High);
        task.ScheduledDate.Should().Be(scheduledDate);
        task.Status.Should().Be(TodoTaskStatus.Pending);
        task.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException(string? invalidTitle)
    {
        // Arrange
        DateOnly scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        Action act = () => new TodoTask(invalidTitle!, scheduledDate, TaskPriority.Low);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithTitleExceedingMaxLength_ThrowsArgumentException()
    {
        // Arrange
        string longTitle = new('x', 201);
        DateOnly scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        Action act = () => new TodoTask(longTitle, scheduledDate, TaskPriority.Low);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeStatus_ToCompleted_SetsCompletedAtUtc()
    {
        // Arrange
        var task = CreateDefaultTask();

        // Act
        task.ChangeStatus(TodoTaskStatus.Completed);

        // Assert
        task.Status.Should().Be(TodoTaskStatus.Completed);
        task.CompletedAtUtc.Should().NotBeNull();
        task.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public void ChangeStatus_WhenAlreadyCompleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var task = CreateDefaultTask();
        task.ChangeStatus(TodoTaskStatus.Completed);

        // Act
        Action act = () => task.ChangeStatus(TodoTaskStatus.InProgress);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateDetails_WhenCancelled_ThrowsInvalidOperationException()
    {
        // Arrange
        var task = CreateDefaultTask();
        task.ChangeStatus(TodoTaskStatus.Cancelled);

        // Act
        Action act = () => task.UpdateDetails(
            "New title", null, TaskPriority.Medium,
            DateOnly.FromDateTime(DateTime.UtcNow), null);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IsOverdue_WhenPastDueAndPending_ReturnsTrue()
    {
        // Arrange
        DateOnly yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var task = new TodoTask("Review PR", yesterday, TaskPriority.Medium);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        bool isOverdue = task.IsOverdue(today);

        // Assert
        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WhenPastDueButCompleted_ReturnsFalse()
    {
        // Arrange
        DateOnly yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        var task = new TodoTask("Review PR", yesterday, TaskPriority.Medium);
        task.ChangeStatus(TodoTaskStatus.Completed);
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act
        bool isOverdue = task.IsOverdue(today);

        // Assert
        isOverdue.Should().BeFalse();
    }

    private static TodoTask CreateDefaultTask()
    {
        return new TodoTask(
            "Write unit tests",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TaskPriority.Medium);
    }
}
