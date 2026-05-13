using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Common;
using CursorAgenticWebApi.Domain.Entities;
using CursorAgenticWebApi.Domain.Enums;

namespace CursorAgenticWebApi.Application.Tasks;

public sealed class TaskApplicationService(ITaskRepository tasks)
{
    public async Task<IReadOnlyList<TaskDto>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await tasks.ListByUserIdAsync(userId, cancellationToken).ConfigureAwait(false);
        return list.Select(Map).ToList();
    }

    public async Task<ServiceResult<TaskDto>> GetAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await tasks.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
        if (task is null || task.UserId != userId)
            return new ServiceResult<TaskDto>.Failure("not_found", "Task was not found.");

        return new ServiceResult<TaskDto>.Success(Map(task));
    }

    public async Task<ServiceResult<TaskDto>> CreateAsync(
        Guid userId,
        CreateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var title = request.Title.Trim();
        var description = (request.Description ?? string.Empty).Trim();
        var validation = ValidateTaskContent(title, description, request.Status);
        if (validation is not null)
            return new ServiceResult<TaskDto>.Failure(validation.Value.Code, validation.Value.Message);

        var now = DateTime.UtcNow;
        var due = request.DueDateUtc ?? now.Date.AddDays(7);

        var entity = new WorkTask
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Description = description,
            Status = request.Status,
            DueDateUtc = due,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await tasks.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
        return new ServiceResult<TaskDto>.Success(Map(entity));
    }

    public async Task<ServiceResult<TaskDto>> UpdateAsync(
        Guid userId,
        Guid taskId,
        UpdateTaskRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await tasks.GetByIdAsync(taskId, cancellationToken).ConfigureAwait(false);
        if (existing is null || existing.UserId != userId)
            return new ServiceResult<TaskDto>.Failure("not_found", "Task was not found.");

        var title = request.Title.Trim();
        var description = (request.Description ?? string.Empty).Trim();
        var validation = ValidateTaskContent(title, description, request.Status);
        if (validation is not null)
            return new ServiceResult<TaskDto>.Failure(validation.Value.Code, validation.Value.Message);

        var now = DateTime.UtcNow;
        var due = request.DueDateUtc ?? existing.DueDateUtc;

        var updated = new WorkTask
        {
            Id = existing.Id,
            UserId = existing.UserId,
            Title = title,
            Description = description,
            Status = request.Status,
            DueDateUtc = due,
            CreatedAtUtc = existing.CreatedAtUtc,
            UpdatedAtUtc = now,
        };

        var ok = await tasks.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
        if (!ok)
            return new ServiceResult<TaskDto>.Failure("conflict", "Task could not be updated.");

        return new ServiceResult<TaskDto>.Success(Map(updated));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var deleted = await tasks.DeleteAsync(taskId, userId, cancellationToken).ConfigureAwait(false);
        if (!deleted)
            return new ServiceResult<bool>.Failure("not_found", "Task was not found.");

        return new ServiceResult<bool>.Success(true);
    }

    private static TaskDto Map(WorkTask t) =>
        new(t.Id, t.UserId, t.Title, t.Description, t.Status, t.DueDateUtc, t.CreatedAtUtc, t.UpdatedAtUtc);

    private static (string Code, string Message)? ValidateTaskContent(string title, string description, TaskStatus status)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 500)
            return ("validation", "Title must be between 1 and 500 characters.");

        if (description.Length > 4000)
            return ("validation", "Description must be at most 4000 characters.");

        if (!Enum.IsDefined(status))
            return ("validation", "Status is not valid.");

        return null;
    }
}
