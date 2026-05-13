using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Application.Auth;
using CursorAgenticWebApi.Application.Common;
using CursorAgenticWebApi.Domain.Entities;
using Moq;
using Xunit;

namespace CursorAgenticWebApi.Application.Tests;

public sealed class UserRegistrationServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasherService> _hasher = new();

    private UserRegistrationService CreateSut() => new(_users.Object, _hasher.Object);

    [Fact]
    public async Task RegisterAsync_returns_validation_when_email_invalid()
    {
        var sut = CreateSut();
        var result = await sut.RegisterAsync(new RegisterUserRequest("bad", "password1", "Name")).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<RegisterUserResponse>.Failure>(result);
        Assert.Equal("validation", failure.Code);
    }

    [Fact]
    public async Task RegisterAsync_returns_validation_when_password_short()
    {
        var sut = CreateSut();
        var result = await sut.RegisterAsync(new RegisterUserRequest("a@b.com", "short", "Name")).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<RegisterUserResponse>.Failure>(result);
        Assert.Equal("validation", failure.Code);
    }

    [Fact]
    public async Task RegisterAsync_returns_conflict_when_email_exists()
    {
        _users.Setup(x => x.GetByEmailAsync("x@y.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "x@y.com", PasswordHash = "h", DisplayName = "Old", CreatedAtUtc = DateTime.UtcNow });

        var sut = CreateSut();
        var result = await sut.RegisterAsync(new RegisterUserRequest("x@y.com", "password1", "Name")).ConfigureAwait(false);

        var failure = Assert.IsType<ServiceResult<RegisterUserResponse>.Failure>(result);
        Assert.Equal("conflict", failure.Code);
    }

    [Fact]
    public async Task RegisterAsync_inserts_user_when_valid()
    {
        _users.Setup(x => x.GetByEmailAsync("new@y.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _hasher.Setup(x => x.HashPassword("password1")).Returns("HASH");

        User? captured = null;
        _users.Setup(x => x.InsertAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => captured = u)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        var result = await sut.RegisterAsync(new RegisterUserRequest("new@y.com", "password1", "Display")).ConfigureAwait(false);

        var success = Assert.IsType<ServiceResult<RegisterUserResponse>.Success>(result);
        Assert.Equal("new@y.com", success.Value.Email);
        Assert.NotEqual(Guid.Empty, success.Value.UserId);
        Assert.NotNull(captured);
        Assert.Equal("HASH", captured!.PasswordHash);
        Assert.Equal("Display", captured.DisplayName);
    }
}
