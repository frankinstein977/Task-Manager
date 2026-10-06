using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;
using TodoTasks.Infrastructure.Persistence;
using TodoTasks.Infrastructure.Repositories;
using Xunit;

namespace TodoTasks.UnitTests.Repositories;

/// <summary>
/// Unit tests for <see cref="TodoTaskRepository"/> query logic, using the
/// EF Core InMemory provider as a lightweight persistence double.
/// </summary>
public class TodoTaskRepositoryTests : IDisposable
{
    private readonly TodoTasksDbContext _dbContext;
    private readonly TodoTaskRepository _sut;

    /// <summary>
    /// Initializes an isolated in-memory database and the repository under test.
    /// </summary>
    public TodoTaskRepositoryTests()
    {
        DbContextOptions<TodoTasksDbContext> options =
            new DbContextOptionsBuilder<TodoTasksDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        _dbContext = new TodoTasksDbContext(options);
        _sut = new TodoTaskRepository(_dbContext);
    }

    [Fact]
    public async Task GetByScheduledDateAsync_ReturnsOnlyTasksForThatDate()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        DateOnly tomorrow = today.AddDays(1);
        await SeedAsync(
            new TodoTask("Task A", today, TaskPriority.Low),
            new TodoTask("Task B", tomorrow, TaskPriority.High),
            new TodoTask("Task C", today, TaskPriority.Medium));

        // Act
        IReadOnlyList<TodoTask> result = await _sut.GetByScheduledDateAsync(today);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(task => task.ScheduledDate == today);
        result.Select(task => task.Title).Should().ContainInOrder("Task C", "Task A");
    }

    [Fact]
    public async Task GetByStatusAsync_ReturnsOnlyMatchingTasks()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        var completedTask = new TodoTask("Done task", today, TaskPriority.Low);
        completedTask.ChangeStatus(TodoTaskStatus.Completed);
        await SeedAsync(
            completedTask,
            new TodoTask("Pending task", today, TaskPriority.Low));

        // Act
        IReadOnlyList<TodoTask> result = await _sut.GetByStatusAsync(TodoTaskStatus.Pending);

        // Assert
        result.Should().ContainSingle();
        result[0].Title.Should().Be("Pending task");
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ExcludesCompletedAndFutureTasks()
    {
        // Arrange
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        var overdueCompleted = new TodoTask("Overdue but done", today.AddDays(-2), TaskPriority.High);
        overdueCompleted.ChangeStatus(TodoTaskStatus.Completed);
        await SeedAsync(
            overdueCompleted,
            new TodoTask("Overdue pending", today.AddDays(-1), TaskPriority.Medium),
            new TodoTask("Future task", today.AddDays(3), TaskPriority.Low));

        // Act
        IReadOnlyList<TodoTask> result = await _sut.GetOverdueTasksAsync(today);

        // Assert
        result.Should().ContainSingle();
        result[0].Title.Should().Be("Overdue pending");
    }

    [Fact]
    public async Task AddAndGetById_RoundTripsEntityThroughUnitOfWork()
    {
        // Arrange
        var unitOfWork = new UnitOfWork(_dbContext);
        var task = new TodoTask(
            "Persisted task", DateOnly.FromDateTime(DateTime.UtcNow), TaskPriority.Critical);

        // Act
        await unitOfWork.TodoTasks.AddAsync(task);
        int written = await unitOfWork.SaveChangesAsync();
        TodoTask? persisted = await unitOfWork.TodoTasks.GetByIdAsync(task.Id);

        // Assert
        written.Should().Be(1);
        persisted.Should().NotBeNull();
        persisted!.Title.Should().Be("Persisted task");
        persisted.Priority.Should().Be(TaskPriority.Critical);
    }

    private async Task SeedAsync(params TodoTask[] tasks)
    {
        await _dbContext.TodoTasks.AddRangeAsync(tasks);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Disposes the in-memory database context.
    /// </summary>
    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
