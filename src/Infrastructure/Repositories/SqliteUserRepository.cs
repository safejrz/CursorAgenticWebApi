using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Domain.Entities;
using Microsoft.Data.Sqlite;

namespace CursorAgenticWebApi.Infrastructure.Repositories;

public sealed class SqliteUserRepository(ISqliteConnectionFactory connections) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Email, PasswordHash, DisplayName, CreatedAtUtc
            FROM Users
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return Map(reader);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Email, PasswordHash, DisplayName, CreatedAtUtc
            FROM Users
            WHERE Email = $email COLLATE NOCASE
            """;
        command.Parameters.AddWithValue("$email", email);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return Map(reader);
    }

    public async Task InsertAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Id, Email, PasswordHash, DisplayName, CreatedAtUtc)
            VALUES ($id, $email, $hash, $display, $created)
            """;
        command.Parameters.AddWithValue("$id", user.Id.ToString());
        command.Parameters.AddWithValue("$email", user.Email);
        command.Parameters.AddWithValue("$hash", user.PasswordHash);
        command.Parameters.AddWithValue("$display", user.DisplayName);
        command.Parameters.AddWithValue("$created", user.CreatedAtUtc.ToString("o"));
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static User Map(SqliteDataReader reader) =>
        new()
        {
            Id = Guid.Parse(reader.GetString(0)),
            Email = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            DisplayName = reader.GetString(3),
            CreatedAtUtc = DateTime.Parse(reader.GetString(4), null, System.Globalization.DateTimeStyles.RoundtripKind),
        };
}
