using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using CursorAgenticWebApi.Infrastructure.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace CursorAgenticWebApi.Tasks.Api.Tests;

public sealed class TasksApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"tasks-api-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", $"Data Source={_dbPath}");
        builder.UseEnvironment("Development");
    }
}

public sealed class TasksApiTests
{
    [Fact]
    public async Task Health_does_not_require_auth()
    {
        await using var factory = new TasksApiFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/health").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Tasks_require_bearer_token()
    {
        await using var factory = new TasksApiFactory();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/tasks").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Tasks_returns_seeded_items_for_demo_user()
    {
        await using var factory = new TasksApiFactory();
        var client = factory.CreateClient();
        var token = TestJwt.CreateAccessToken(DataSeeder.DemoUserId, DataSeeder.DemoEmail, DataSeeder.DemoDisplayName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("/api/tasks").ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        var tasks = await JsonSerializer.DeserializeAsync<JsonElement>(stream).ConfigureAwait(false);
        Assert.Equal(JsonValueKind.Array, tasks.ValueKind);
        Assert.True(tasks.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task Crud_create_update_delete()
    {
        await using var factory = new TasksApiFactory();
        var client = factory.CreateClient();
        var token = TestJwt.CreateAccessToken(DataSeeder.DemoUserId, DataSeeder.DemoEmail, DataSeeder.DemoDisplayName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var create = await client.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                title = "Integration task",
                description = "from tests",
                status = "Pending",
                dueDateUtc = (DateTime?)DateTime.UtcNow.AddDays(3),
            }).ConfigureAwait(false);

        create.EnsureSuccessStatusCode();
        var created = await create.Content.ReadFromJsonAsync<JsonElement>().ConfigureAwait(false);
        var id = created.GetProperty("id").GetGuid();

        var update = await client.PutAsJsonAsync(
            $"/api/tasks/{id}",
            new
            {
                title = "Integration task updated",
                description = "from tests",
                status = "InProgress",
                dueDateUtc = (DateTime?)DateTime.UtcNow.AddDays(4),
            }).ConfigureAwait(false);

        update.EnsureSuccessStatusCode();

        var delete = await client.DeleteAsync($"/api/tasks/{id}").ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
    }

    private static class TestJwt
    {
        public static string CreateAccessToken(Guid userId, string email, string displayName)
        {
            var signingKey = "dev-signing-key-change-me-32chars-min!!";
            var issuer = "https://localhost:5001";
            var audience = "CursorAgenticWebApi";
            var expires = DateTime.UtcNow.AddHours(1);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("display_name", displayName),
            };

            var token = new JwtSecurityToken(
                issuer,
                audience,
                claims,
                notBefore: DateTime.UtcNow.AddMinutes(-1),
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
