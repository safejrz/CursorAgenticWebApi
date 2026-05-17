using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Domain.Entities;
using CursorAgenticWebApi.Domain.Enums;

namespace CursorAgenticWebApi.Infrastructure.Seeding;

public sealed class DataSeeder(
    IUserRepository users,
    ITaskRepository tasks,
    IPasswordHasherService passwordHasher) : IDataSeeder
{
    public static readonly Guid DemoUserId = Guid.Parse("11111111-1111-4111-8111-111111111111");

    public const string DemoEmail = "demo@example.com";
    public const string DemoPassword = "DemoPass1!";
    public const string DemoDisplayName = "Demo User";

    public async Task SeedDemoDataIfEmptyAsync(CancellationToken cancellationToken = default)
    {
        var existing = await users.GetByEmailAsync(DemoEmail, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return;

        var user = new User
        {
            Id = DemoUserId,
            Email = DemoEmail,
            PasswordHash = passwordHasher.HashPassword(DemoPassword),
            DisplayName = DemoDisplayName,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await users.InsertAsync(user, cancellationToken).ConfigureAwait(false);

        var now = DateTime.UtcNow;
        var seedTasks = new[]
        {
            new WorkTask
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Prepare presentation",
                Description = "Summarize architecture and GenAI workflow.",
                Status = WorkTaskStatus.InProgress,
                DueDateUtc = now.AddDays(2),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
            new WorkTask
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Run automated tests",
                Description = "Ensure dotnet test and npm test pass locally.",
                Status = WorkTaskStatus.Pending,
                DueDateUtc = now.AddDays(1),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
            new WorkTask
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Review API contracts",
                Description = "Confirm CRUD routes and auth headers for the SPA.",
                Status = WorkTaskStatus.Done,
                DueDateUtc = now.AddDays(-1),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
            },
        };

        foreach (var t in seedTasks)
            await tasks.InsertAsync(t, cancellationToken).ConfigureAwait(false);
    }
}
