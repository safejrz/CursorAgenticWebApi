using CursorAgenticWebApi.Domain.Entities;

namespace CursorAgenticWebApi.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task InsertAsync(User user, CancellationToken cancellationToken = default);
}
