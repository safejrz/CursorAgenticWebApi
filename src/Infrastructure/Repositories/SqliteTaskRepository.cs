using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Domain.Entities;
using CursorAgenticWebApi.Domain.Enums;
using Microsoft.Data.Sqlite;

namespace CursorAgenticWebApi.Infrastructure.Repositories;

public sealed class SqliteTaskRepository(ISqliteConnectionFactory connections) : ITaskRepository
{
    public async Task<IReadOnlyList<WorkTask>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc
            FROM Tasks
            WHERE UserId = $userId
            ORDER BY DueDateUtc
            """;
        command.Parameters.AddWithValue("$userId", userId.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<WorkTask>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(Map(reader));

        return list;
    }

    public async Task<WorkTask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc
            FROM Tasks
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$id", id.ToString());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return Map(reader);
    }

    public async Task InsertAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Tasks (Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc)
            VALUES ($id, $userId, $title, $desc, $status, $due, $created, $updated)
            """;
        AddParameters(command, task);
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Tasks
            SET Title = $title,
                Description = $desc,
                Status = $status,
                DueDateUtc = $due,
                UpdatedAtUtc = $updated
            WHERE Id = $id AND UserId = $userId
            """;
        AddParameters(command, task);
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        await using var connection = connections.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE Id = $id AND UserId = $userId";
        command.Parameters.AddWithValue("$id", id.ToString());
        command.Parameters.AddWithValue("$userId", userId.ToString());
        var rows = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return rows > 0;
    }

    private static void AddParameters(SqliteCommand command, WorkTask task)
    {
        command.Parameters.AddWithValue("$id", task.Id.ToString());
        command.Parameters.AddWithValue("$userId", task.UserId.ToString());
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$desc", task.Description);
        command.Parameters.AddWithValue("$status", (int)task.Status);
        command.Parameters.AddWithValue("$due", task.DueDateUtc.ToString("o"));
        command.Parameters.AddWithValue("$created", task.CreatedAtUtc.ToString("o"));
        command.Parameters.AddWithValue("$updated", task.UpdatedAtUtc.ToString("o"));
    }

    private static WorkTask Map(SqliteDataReader reader) =>
        new()
        {
            Id = Guid.Parse(reader.GetString(0)),
            UserId = Guid.Parse(reader.GetString(1)),
            Title = reader.GetString(2),
            Description = reader.GetString(3),
            Status = (TaskStatus)reader.GetInt32(4),
            DueDateUtc = DateTime.Parse(reader.GetString(5), null, System.Globalization.DateTimeStyles.RoundtripKind),
            CreatedAtUtc = DateTime.Parse(reader.GetString(6), null, System.Globalization.DateTimeStyles.RoundtripKind),
            UpdatedAtUtc = DateTime.Parse(reader.GetString(7), null, System.Globalization.DateTimeStyles.RoundtripKind),
        };
}
