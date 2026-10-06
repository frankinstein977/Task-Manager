using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TodoTasks.Application.DTOs;
using TodoTasks.Application.Exceptions;
using TodoTasks.Application.Services;
using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;
using TodoTasks.Domain.Interfaces;
using Xunit;

namespace TodoTasks.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="TodoTaskService"/> using mocked Unit of Work
/// and repository dependencies.
/// </summary>
public class TodoTaskServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ITodoTaskRepository> _repositoryMock;
    private readonly TodoTaskService _sut;

    /// <summary>
    /// Initializes the shared test fixture: mocked dependencies and the system under test.
    /// </summary>
    public TodoTaskServiceTests()
    {
        _repositoryMock = new Mock<ITodoTaskRepository>(MockBehavior.Strict);
        _unitOfWorkMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _unitOfWorkMock.SetupGet(uow => uow.TodoTasks).Returns(_repositoryMock.Object);

        var loggerMock = new Mock<ILogger<TodoTaskService>>();
        _sut = new TodoTaskService(_unitOfWorkMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_WithValidRequest_PersistsAndReturnsTask()
    {
        // Arrange
        var request = new CreateTodoTaskRequest(
            "Deploy hotfix", "Roll out fix for ticket INC-42",
            TaskPriority.Critical, DateOnly.FromDateTime(DateTime.UtcNow), "bob");

        _repositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<TodoTask>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        TodoTaskResponse result = await _sut.CreateAsync(request);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Title.Should().Be("Deploy hotfix");
        result.Priority.Should().Be(TaskPriority.Critical);
        result.Status.Should().Be(TodoTaskStatus.Pending);

        _repositoryMock.Verify(
            repo => repo.AddAsync(
                It.Is<TodoTask>(t => t.Title == request.Title),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var existingTask = new TodoTask(
            "Refactor repository layer",
            DateOnly.FromDateTime(DateTime.UtcNow),
            TaskPriority.Medium);

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(existingTask.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);

        // Act
        TodoTaskResponse result = await _sut.GetByIdAsync(existingTask.Id);

        // Assert
        result.Id.Should().Be(existingTask.Id);
        result.Title.Should().Be("Refactor repository layer");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTaskMissing_ThrowsNotFoundException()
    {
        // Arrange
        Guid missingId = Guid.NewGuid();
        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoTask?)null);

        // Act
        Func<Task> act = () => _sut.GetByIdAsync(missingId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllAsync_WithScheduledDateFilter_QueriesRepositoryByDate()
    {
        // Arrange
        DateOnly scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<TodoTask> tasks = new List<TodoTask>
        {
            new("Morning standup follow-ups", scheduledDate, TaskPriority.Low)
        };

        _repositoryMock
            .Setup(repo => repo.GetByScheduledDateAsync(scheduledDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        // Act
        IReadOnlyList<TodoTaskResponse> result = await _sut.GetAllAsync(scheduledDate, null);

        // Assert
        result.Should().HaveCount(1);
        _repositoryMock.Verify(
            repo => repo.GetByScheduledDateAsync(scheduledDate, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskExists_UpdatesAndPersists()
    {
        // Arrange
        var existingTask = new TodoTask(
            "Old title", DateOnly.FromDateTime(DateTime.UtcNow), TaskPriority.Low);
        var request = new UpdateTodoTaskRequest(
            "New title", "Updated description", TaskPriority.High,
            DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), "carol");

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(existingTask.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _repositoryMock.Setup(repo => repo.Update(existingTask));
        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        TodoTaskResponse result = await _sut.UpdateAsync(existingTask.Id, request);

        // Assert
        result.Title.Should().Be("New title");
        result.Priority.Should().Be(TaskPriority.High);
        _unitOfWorkMock.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenTaskAlreadyCompleted_ThrowsBusinessRuleViolation()
    {
        // Arrange
        var completedTask = new TodoTask(
            "Finished task", DateOnly.FromDateTime(DateTime.UtcNow), TaskPriority.Low);
        completedTask.ChangeStatus(TodoTaskStatus.Completed);
        var request = new ChangeTaskStatusRequest(TodoTaskStatus.InProgress);

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(completedTask.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(completedTask);

        // Act
        Func<Task> act = () => _sut.ChangeStatusAsync(completedTask.Id, request);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
        _unitOfWorkMock.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskExists_RemovesAndPersists()
    {
        // Arrange
        var existingTask = new TodoTask(
            "Obsolete task", DateOnly.FromDateTime(DateTime.UtcNow), TaskPriority.Low);

        _repositoryMock
            .Setup(repo => repo.GetByIdAsync(existingTask.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTask);
        _repositoryMock.Setup(repo => repo.Remove(existingTask));
        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.DeleteAsync(existingTask.Id);

        // Assert
        _repositoryMock.Verify(repo => repo.Remove(existingTask), Times.Once);
        _unitOfWorkMock.Verify(
            uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOverdueAsync_DelegatesToRepositoryWithCurrentDate()
    {
        // Arrange
        DateOnly yesterday = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1);
        IReadOnlyList<TodoTask> overdueTasks = new List<TodoTask>
        {
            new("Overdue migration", yesterday, TaskPriority.High)
        };

        _repositoryMock
            .Setup(repo => repo.GetOverdueTasksAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overdueTasks);

        // Act
        IReadOnlyList<TodoTaskResponse> result = await _sut.GetOverdueAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].IsOverdue.Should().BeTrue();
    }
}
