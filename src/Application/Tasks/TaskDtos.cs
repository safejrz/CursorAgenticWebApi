using CursorAgenticWebApi.Domain.Enums;

namespace CursorAgenticWebApi.Application.Tasks;

public sealed record TaskDto(
    Guid Id,
    Guid UserId,
    string Title,
    string Description,
    WorkTaskStatus Status,
    DateTime DueDateUtc,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateTaskRequest(string Title, string Description, WorkTaskStatus Status, DateTime? DueDateUtc);

public sealed record UpdateTaskRequest(string Title, string Description, WorkTaskStatus Status, DateTime? DueDateUtc);
