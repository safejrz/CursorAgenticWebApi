namespace CursorAgenticWebApi.Application.Auth;

public sealed record UserProfileResponse(Guid UserId, string Email, string DisplayName, DateTime CreatedAtUtc);
