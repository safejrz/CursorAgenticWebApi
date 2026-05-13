namespace CursorAgenticWebApi.Application.Auth;

public sealed record RegisterUserRequest(string Email, string Password, string DisplayName);

public sealed record RegisterUserResponse(Guid UserId, string Email, string DisplayName);
