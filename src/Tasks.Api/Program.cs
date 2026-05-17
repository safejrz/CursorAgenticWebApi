using CursorAgenticWebApi.Application;
using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Infrastructure;
using CursorAgenticWebApi.Infrastructure.Security;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

ConfigureSharedDatabasePath(builder);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var frontendOrigin = builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "spa",
        policy => policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bootstrapper = scope.ServiceProvider.GetRequiredService<IDatabaseBootstrapper>();
    await bootstrapper.EnsureDatabaseAsync().ConfigureAwait(false);

    if (ShouldSeedDemoData(app))
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedDemoDataIfEmptyAsync().ConfigureAwait(false);
    }
}

app.UseCors("spa");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

static void ConfigureSharedDatabasePath(WebApplicationBuilder builder)
{
    var existing = builder.Configuration.GetConnectionString("Database");
    if (!string.IsNullOrWhiteSpace(existing))
        return;

    var dataDir = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
    Directory.CreateDirectory(dataDir);
    var dbPath = Path.Combine(dataDir, "app.db");
    builder.Configuration["ConnectionStrings:Database"] = $"Data Source={dbPath}";
}

static bool ShouldSeedDemoData(WebApplication app) =>
    app.Environment.IsDevelopment()
    || app.Configuration.GetValue("Demo:SeedIfEmpty", false);

public partial class Program { }
