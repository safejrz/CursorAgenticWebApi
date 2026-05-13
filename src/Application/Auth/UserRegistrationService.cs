using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Common;
using CursorAgenticWebApi.Domain.Entities;

namespace CursorAgenticWebApi.Application.Auth;

public sealed class UserRegistrationService(
    IUserRepository users,
    IPasswordHasherService passwordHasher)
{
    public async Task<ServiceResult<RegisterUserResponse>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (string.IsNullOrWhiteSpace(email))
            return new ServiceResult<RegisterUserResponse>.Failure("validation", "Email is required.");

        if (email.Length > 320 || !email.Contains('@', StringComparison.Ordinal))
            return new ServiceResult<RegisterUserResponse>.Failure("validation", "Email is not valid.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            return new ServiceResult<RegisterUserResponse>.Failure("validation", "Password must be at least 8 characters.");

        var displayName = request.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200)
            return new ServiceResult<RegisterUserResponse>.Failure("validation", "Display name must be between 1 and 200 characters.");

        var existing = await users.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return new ServiceResult<RegisterUserResponse>.Failure("conflict", "A user with this email already exists.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            DisplayName = displayName,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await users.InsertAsync(user, cancellationToken).ConfigureAwait(false);

        return new ServiceResult<RegisterUserResponse>.Success(new RegisterUserResponse(user.Id, user.Email, user.DisplayName));
    }
}
