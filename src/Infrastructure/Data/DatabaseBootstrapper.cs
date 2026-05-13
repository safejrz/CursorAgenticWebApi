using System.Reflection;
using CursorAgenticWebApi.Application.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace CursorAgenticWebApi.Infrastructure.Data;

public sealed class DatabaseBootstrapper(
    ISqliteConnectionFactory connectionFactory,
    ILogger<DatabaseBootstrapper> logger) : IDatabaseBootstrapper
{
    public async Task EnsureDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("CursorAgenticWebApi.Infrastructure.Scripts.init.sql")
            ?? throw new InvalidOperationException("Embedded resource init.sql was not found.");

        using var reader = new StreamReader(stream);
        var sql = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

        await using var connection = connectionFactory.CreateOpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        logger.LogInformation("SQLite schema ensured.");
    }
}
