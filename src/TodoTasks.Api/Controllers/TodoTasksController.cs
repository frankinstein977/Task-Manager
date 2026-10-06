using Microsoft.AspNetCore.Mvc;
using TodoTasks.Application.DTOs;
using TodoTasks.Application.Services;
using TodoTasks.Domain.Enums;

namespace TodoTasks.Api.Controllers;

/// <summary>
/// REST endpoints for tracking daily technical tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TodoTasksController : ControllerBase
{
    private readonly ITodoTaskService _todoTaskService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TodoTasksController"/> class.
    /// </summary>
    /// <param name="todoTaskService">The task application service.</param>
    public TodoTasksController(ITodoTaskService todoTaskService)
    {
        _todoTaskService = todoTaskService ?? throw new ArgumentNullException(nameof(todoTaskService));
    }

    /// <summary>
    /// Gets all tasks, optionally filtered by scheduled date and/or status.
    /// </summary>
    /// <param name="scheduledDate">Optional scheduled date filter (YYYY-MM-DD).</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The list of matching tasks.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TodoTaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TodoTaskResponse>>> GetAll(
        [FromQuery] DateOnly? scheduledDate,
        [FromQuery] TodoTaskStatus? status,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TodoTaskResponse> tasks =
            await _todoTaskService.GetAllAsync(scheduledDate, status, cancellationToken);

        return Ok(tasks);
    }

    /// <summary>
    /// Gets all overdue, unfinished tasks.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The list of overdue tasks.</returns>
    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IReadOnlyList<TodoTaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TodoTaskResponse>>> GetOverdue(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<TodoTaskResponse> tasks =
            await _todoTaskService.GetOverdueAsync(cancellationToken);

        return Ok(tasks);
    }

    /// <summary>
    /// Gets a single task by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the task.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The requested task.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TodoTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoTaskResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        TodoTaskResponse task = await _todoTaskService.GetByIdAsync(id, cancellationToken);
        return Ok(task);
    }

    /// <summary>
    /// Creates a new technical task.
    /// </summary>
    /// <param name="request">The creation payload.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The created task.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TodoTaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoTaskResponse>> Create(
        [FromBody] CreateTodoTaskRequest request,
        CancellationToken cancellationToken)
    {
        TodoTaskResponse created = await _todoTaskService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing technical task.
    /// </summary>
    /// <param name="id">The identifier of the task to update.</param>
    /// <param name="request">The update payload.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated task.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TodoTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoTaskResponse>> Update(
        Guid id,
        [FromBody] UpdateTodoTaskRequest request,
        CancellationToken cancellationToken)
    {
        TodoTaskResponse updated = await _todoTaskService.UpdateAsync(id, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Transitions a task to a new status.
    /// </summary>
    /// <param name="id">The identifier of the task.</param>
    /// <param name="request">The status change payload.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated task.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(TodoTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoTaskResponse>> ChangeStatus(
        Guid id,
        [FromBody] ChangeTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        TodoTaskResponse updated =
            await _todoTaskService.ChangeStatusAsync(id, request, cancellationToken);

        return Ok(updated);
    }

    /// <summary>
    /// Deletes a task permanently.
    /// </summary>
    /// <param name="id">The identifier of the task to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _todoTaskService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
