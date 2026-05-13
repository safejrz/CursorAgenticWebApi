using CursorAgenticWebApi.Domain.Enums;

namespace CursorAgenticWebApi.Domain.Entities;

public sealed class WorkTask
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TaskStatus Status { get; init; }
    public DateTime DueDateUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}
