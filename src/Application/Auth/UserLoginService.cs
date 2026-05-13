using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Common;

namespace CursorAgenticWebApi.Application.Auth;

public sealed class UserLoginService(
    IUserRepository users,
    IPasswordHasherService passwordHasher,
    IJwtTokenGenerator jwt)
{
    public async Task<ServiceResult<LoginResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(request.Password))
            return new ServiceResult<LoginResponse>.Failure("validation", "Email and password are required.");

        var user = await users.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            return new ServiceResult<LoginResponse>.Failure("invalid_credentials", "Invalid email or password.");

        var issued = jwt.CreateToken(user.Id, user.Email, user.DisplayName);

        return new ServiceResult<LoginResponse>.Success(
            new LoginResponse(issued.Token, user.Id, user.Email, user.DisplayName, issued.ExpiresAtUtc));
    }
}
