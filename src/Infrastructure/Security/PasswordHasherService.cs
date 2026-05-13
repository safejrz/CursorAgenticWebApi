using CursorAgenticWebApi.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace CursorAgenticWebApi.Infrastructure.Security;

public sealed class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string HashPassword(string password) => _hasher.HashPassword(string.Empty, password);

    public bool VerifyPassword(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(string.Empty, passwordHash, password) != PasswordVerificationResult.Failed;
}
