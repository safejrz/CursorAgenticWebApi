using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Auth;
using CursorAgenticWebApi.Application.Common;
using CursorAgenticWebApi.Domain.Entities;
using Moq;
using Xunit;

namespace CursorAgenticWebApi.Application.Tests;

public sealed class UserLoginServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasherService> _hasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwt = new();

    private UserLoginService CreateSut() => new(_users.Object, _hasher.Object, _jwt.Object);

    [Fact]
    public async Task LoginAsync_returns_invalid_credentials_when_user_missing()
    {
        _users.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest("a@b.com", "password1")).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<LoginResponse>.Failure>(result);
        Assert.Equal("invalid_credentials", failure.Code);
    }

    [Fact]
    public async Task LoginAsync_returns_invalid_credentials_when_password_wrong()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            PasswordHash = "HASH",
            DisplayName = "N",
            CreatedAtUtc = DateTime.UtcNow,
        };
        _users.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(x => x.VerifyPassword("password1", "HASH")).Returns(false);

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest("a@b.com", "password1")).ConfigureAwait(false);

        Assert.IsType<ServiceResult<LoginResponse>.Failure>(result);
    }

    [Fact]
    public async Task LoginAsync_returns_token_when_valid()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            PasswordHash = "HASH",
            DisplayName = "N",
            CreatedAtUtc = DateTime.UtcNow,
        };
        _users.Setup(x => x.GetByEmailAsync("a@b.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _hasher.Setup(x => x.VerifyPassword("password1", "HASH")).Returns(true);
        var expires = DateTime.UtcNow.AddMinutes(30);
        _jwt.Setup(x => x.CreateToken(user.Id, user.Email, user.DisplayName))
            .Returns(new JwtTokenResult("TOKEN", expires));

        var sut = CreateSut();
        var result = await sut.LoginAsync(new LoginRequest("a@b.com", "password1")).ConfigureAwait(false);

        var success = Assert.IsType<ServiceResult<LoginResponse>.Success>(result);
        Assert.Equal("TOKEN", success.Value.AccessToken);
        Assert.Equal(user.Id, success.Value.UserId);
    }
}
