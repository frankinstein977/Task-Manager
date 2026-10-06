using Microsoft.Extensions.Logging;
using TodoTasks.Application.DTOs;
using TodoTasks.Application.Exceptions;
using TodoTasks.Application.Mappings;
using TodoTasks.Domain.Entities;
using TodoTasks.Domain.Enums;
using TodoTasks.Domain.Interfaces;

namespace TodoTasks.Application.Services;

/// <summary>
/// Implements the application use cases for managing daily technical tasks.
/// Persistence is coordinated through the <see cref="IUnitOfWork"/> abstraction so
/// each use case commits atomically.
/// </summary>
public class TodoTaskService : ITodoTaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TodoTaskService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoTaskService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work coordinating repository access.</param>
    /// <param name="logger">The structured logger instance.</param>
    public TodoTaskService(IUnitOfWork unitOfWork, ILogger<TodoTaskService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<TodoTaskResponse> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        TodoTask task = await GetTaskOrThrowAsync(id, cancellationToken);
        return task.ToResponse();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TodoTaskResponse>> GetAllAsync(
        DateOnly? scheduledDate = null,
        TodoTaskStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<TodoTask> tasks;

        if (scheduledDate.HasValue)
        {
            tasks = await _unitOfWork.TodoTasks
                .GetByScheduledDateAsync(scheduledDate.Value, cancellationToken);
        }
        else if (status.HasValue)
        {
            tasks = await _unitOfWork.TodoTasks
                .GetByStatusAsync(status.Value, cancellationToken);
        }
        else
        {
            tasks = await _unitOfWork.TodoTasks.GetAllAsync(cancellationToken: cancellationToken);
        }

        IReadOnlyList<TodoTaskResponse> responses = tasks
            .Where(t => !status.HasValue || t.Status == status.Value)
            .Select(TodoTaskMappings.ToResponse)
            .ToList();

        _logger.LogInformation(
            "Retrieved {TaskCount} tasks (ScheduledDate={ScheduledDate}, Status={Status}).",
            responses.Count, scheduledDate, status);

        return responses;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TodoTaskResponse>> GetOverdueAsync(
        CancellationToken cancellationToken = default)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<TodoTask> overdueTasks =
            await _unitOfWork.TodoTasks.GetOverdueTasksAsync(today, cancellationToken);

        return overdueTasks.Select(TodoTaskMappings.ToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<TodoTaskResponse> CreateAsync(
        CreateTodoTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var task = new TodoTask(
            request.Title,
            request.ScheduledDate,
            request.Priority,
            request.Description,
            request.Assignee);

        await _unitOfWork.TodoTasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created task {TaskId} ('{TaskTitle}') scheduled for {ScheduledDate}.",
            task.Id, task.Title, task.ScheduledDate);

        return task.ToResponse();
    }

    /// <inheritdoc />
    public async Task<TodoTaskResponse> UpdateAsync(
        Guid id,
        UpdateTodoTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask task = await GetTaskOrThrowAsync(id, cancellationToken);

        try
        {
            task.UpdateDetails(
                request.Title,
                request.Description,
                request.Priority,
                request.ScheduledDate,
                request.Assignee);
        }
        catch (InvalidOperationException exception)
        {
            throw new BusinessRuleViolationException(exception.Message);
        }

        _unitOfWork.TodoTasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated task {TaskId}.", task.Id);

        return task.ToResponse();
    }

    /// <inheritdoc />
    public async Task<TodoTaskResponse> ChangeStatusAsync(
        Guid id,
        ChangeTaskStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        TodoTask task = await GetTaskOrThrowAsync(id, cancellationToken);

        try
        {
            task.ChangeStatus(request.Status);
        }
        catch (InvalidOperationException exception)
        {
            throw new BusinessRuleViolationException(exception.Message);
        }

        _unitOfWork.TodoTasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Task {TaskId} transitioned to status {TodoTaskStatus}.", task.Id, request.Status);

        return task.ToResponse();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        TodoTask task = await GetTaskOrThrowAsync(id, cancellationToken);

        _unitOfWork.TodoTasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted task {TaskId}.", id);
    }

    private async Task<TodoTask> GetTaskOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        TodoTask? task = await _unitOfWork.TodoTasks.GetByIdAsync(id, cancellationToken);
        if (task is null)
        {
            _logger.LogWarning("Task {TaskId} was requested but not found.", id);
            throw new NotFoundException(nameof(TodoTask), id);
        }

        return task;
    }
}
