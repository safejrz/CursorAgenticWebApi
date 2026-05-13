using CursorAgenticWebApi.Domain.Entities;

namespace CursorAgenticWebApi.Application.Abstractions;

public interface ITaskRepository
{
    Task<IReadOnlyList<WorkTask>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task InsertAsync(WorkTask task, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}
