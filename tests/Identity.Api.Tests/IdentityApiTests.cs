using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CursorAgenticWebApi.Identity.Api.Tests;

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"identity-api-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Database", $"Data Source={_dbPath}");
        builder.UseEnvironment("Development");
    }
}

public sealed class IdentityApiTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client;

    public IdentityApiTests(IdentityApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_does_not_require_auth()
    {
        var response = await _client.GetAsync(new Uri("/api/health", UriKind.Relative)).ConfigureAwait(false);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_login_and_me_roundtrip()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var register = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new { email, password = "Password1!", displayName = "Tester" }).ConfigureAwait(false);

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { email, password = "Password1!" }).ConfigureAwait(false);

        login.EnsureSuccessStatusCode();
        var loginBody = await login.Content.ReadFromJsonAsync<LoginPayload>().ConfigureAwait(false);
        Assert.NotNull(loginBody?.accessToken);

        using var meRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        meRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginBody!.accessToken);
        var me = await _client.SendAsync(meRequest).ConfigureAwait(false);
        me.EnsureSuccessStatusCode();
    }

    private sealed record LoginPayload(string accessToken, Guid userId, string email, string displayName, DateTime expiresAtUtc);
}
