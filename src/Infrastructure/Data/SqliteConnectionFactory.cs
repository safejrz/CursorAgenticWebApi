using Microsoft.Extensions.Configuration;

namespace CursorAgenticWebApi.Infrastructure.Data;

public interface ISqliteConnectionFactory
{
    Microsoft.Data.Sqlite.SqliteConnection CreateOpenConnection();
}

public sealed class SqliteConnectionFactory(IConfiguration configuration) : ISqliteConnectionFactory
{
    public Microsoft.Data.Sqlite.SqliteConnection CreateOpenConnection()
    {
        var cs = configuration.GetConnectionString("Database")
                 ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");
        var connection = new Microsoft.Data.Sqlite.SqliteConnection(cs);
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
