namespace CursorAgenticWebApi.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, Guid UserId, string Email, string DisplayName, DateTime ExpiresAtUtc);
