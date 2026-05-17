using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Domain.Entities;
using CursorAgenticWebApi.Domain.Enums;
using CursorAgenticWebApi.Infrastructure;
using CursorAgenticWebApi.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CursorAgenticWebApi.Infrastructure.Tests;

public sealed class SqliteRepositoriesTests : IAsyncLifetime
{
    private string _dbPath = null!;
    private ServiceProvider _provider = null!;

    public async Task InitializeAsync()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"ca-tests-{Guid.NewGuid():N}.db");
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = $"Data Source={_dbPath}",
                ["Jwt:Issuer"] = "test",
                ["Jwt:Audience"] = "test",
                ["Jwt:SigningKey"] = "unit-test-signing-key-at-least-32-chars",
                ["Jwt:ExpiryMinutes"] = "60",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddInfrastructure(configuration);
        _provider = services.BuildServiceProvider();

        await _provider.GetRequiredService<IDatabaseBootstrapper>().EnsureDatabaseAsync().ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync().ConfigureAwait(false);
        try
        {
            if (File.Exists(_dbPath))
                File.Delete(_dbPath);
        }
        catch
        {
            // ignore temp cleanup races
        }
    }

    [Fact]
    public async Task User_roundtrip_insert_and_get_by_email()
    {
        var users = _provider.GetRequiredService<IUserRepository>();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "repo@example.com",
            PasswordHash = "HASH",
            DisplayName = "R",
            CreatedAtUtc = DateTime.UtcNow,
        };

        await users.InsertAsync(user).ConfigureAwait(false);
        var loaded = await users.GetByEmailAsync("REPO@EXAMPLE.COM").ConfigureAwait(false);

        Assert.NotNull(loaded);
        Assert.Equal(user.Id, loaded!.Id);
        Assert.Equal(user.Email, loaded.Email);
    }

    [Fact]
    public async Task Task_crud_scoped_to_user()
    {
        var users = _provider.GetRequiredService<IUserRepository>();
        var tasks = _provider.GetRequiredService<ITaskRepository>();

        var userId = Guid.NewGuid();
        await users.InsertAsync(
            new User
            {
                Id = userId,
                Email = $"u{Guid.NewGuid():N}@e.com",
                PasswordHash = "H",
                DisplayName = "U",
                CreatedAtUtc = DateTime.UtcNow,
            }).ConfigureAwait(false);

        var task = new WorkTask
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = "T",
            Description = "D",
            Status = WorkTaskStatus.Pending,
            DueDateUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        await tasks.InsertAsync(task).ConfigureAwait(false);
        var list = await tasks.ListByUserIdAsync(userId).ConfigureAwait(false);
        Assert.Single(list);

        var updated = new WorkTask
        {
            Id = task.Id,
            UserId = task.UserId,
            Title = "T2",
            Description = task.Description,
            Status = task.Status,
            DueDateUtc = task.DueDateUtc,
            CreatedAtUtc = task.CreatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow,
        };
        Assert.True(await tasks.UpdateAsync(updated).ConfigureAwait(false));

        Assert.True(await tasks.DeleteAsync(task.Id, userId).ConfigureAwait(false));
        Assert.Empty(await tasks.ListByUserIdAsync(userId).ConfigureAwait(false));
    }
}
