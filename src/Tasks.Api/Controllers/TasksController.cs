using System.Security.Claims;
using CursorAgenticWebApi.Application.Common;
using CursorAgenticWebApi.Application.Tasks;
using CursorAgenticWebApi.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursorAgenticWebApi.Tasks.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(TaskApplicationService tasks) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        if (userId is null)
            return Unauthorized();

        var list = await tasks.ListAsync(userId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(list);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        if (userId is null)
            return Unauthorized();

        var result = await tasks.GetAsync(userId.Value, id, cancellationToken).ConfigureAwait(false);
        return Map(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaskApiRequest body, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        if (userId is null)
            return Unauthorized();

        var request = new CreateTaskRequest(body.Title, body.Description ?? string.Empty, body.Status, body.DueDateUtc);
        var result = await tasks.CreateAsync(userId.Value, request, cancellationToken).ConfigureAwait(false);

        return result switch
        {
            ServiceResult<TaskDto>.Success s => Created($"/api/tasks/{s.Value.Id}", s.Value),
            ServiceResult<TaskDto>.Failure f => BadRequest(Problem(f.Message)),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskApiRequest body, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        if (userId is null)
            return Unauthorized();

        var request = new UpdateTaskRequest(body.Title, body.Description ?? string.Empty, body.Status, body.DueDateUtc);
        var result = await tasks.UpdateAsync(userId.Value, id, request, cancellationToken).ConfigureAwait(false);

        return result switch
        {
            ServiceResult<TaskDto>.Success s => Ok(s.Value),
            ServiceResult<TaskDto>.Failure f => f.Code == "not_found"
                ? NotFound(Problem(f.Message, StatusCodes.Status404NotFound))
                : BadRequest(Problem(f.Message)),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        if (userId is null)
            return Unauthorized();

        var result = await tasks.DeleteAsync(userId.Value, id, cancellationToken).ConfigureAwait(false);

        return result switch
        {
            ServiceResult<bool>.Success => NoContent(),
            ServiceResult<bool>.Failure f => NotFound(Problem(f.Message, StatusCodes.Status404NotFound)),
            _ => Problem(statusCode: StatusCodes.Status500InternalServerError),
        };
    }

    private Guid? RequireUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : null;
    }

    private static IActionResult Map(ServiceResult<TaskDto> result) =>
        result switch
        {
            ServiceResult<TaskDto>.Success s => new OkObjectResult(s.Value),
            ServiceResult<TaskDto>.Failure f => new NotFoundObjectResult(
                new ProblemDetails { Title = "Not Found", Detail = f.Message, Status = StatusCodes.Status404NotFound }),
            _ => new ObjectResult("Unexpected result") { StatusCode = StatusCodes.Status500InternalServerError },
        };

    private static ProblemDetails Problem(string detail, int status = StatusCodes.Status400BadRequest) =>
        new() { Title = status == StatusCodes.Status404NotFound ? "Not Found" : "Bad Request", Detail = detail, Status = status };
}

public sealed record CreateTaskApiRequest(string Title, string? Description, TaskStatus Status, DateTime? DueDateUtc);

public sealed record UpdateTaskApiRequest(string Title, string? Description, TaskStatus Status, DateTime? DueDateUtc);
