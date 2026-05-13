using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Common;

namespace CursorAgenticWebApi.Application.Auth;

public sealed class UserProfileService(IUserRepository users)
{
    public async Task<ServiceResult<UserProfileResponse>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return new ServiceResult<UserProfileResponse>.Failure("validation", "User id is required.");

        var user = await users.GetByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return new ServiceResult<UserProfileResponse>.Failure("not_found", "User was not found.");

        return new ServiceResult<UserProfileResponse>.Success(
            new UserProfileResponse(user.Id, user.Email, user.DisplayName, user.CreatedAtUtc));
    }
}
