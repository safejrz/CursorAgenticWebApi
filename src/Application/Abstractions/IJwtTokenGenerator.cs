namespace CursorAgenticWebApi.Application.Abstractions;

public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);

public interface IJwtTokenGenerator
{
    JwtTokenResult CreateToken(Guid userId, string email, string displayName);
}
