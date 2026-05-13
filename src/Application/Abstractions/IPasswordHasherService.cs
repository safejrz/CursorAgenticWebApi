namespace CursorAgenticWebApi.Application.Abstractions;

public interface IPasswordHasherService
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string passwordHash);
}
