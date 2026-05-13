using CursorAgenticWebApi.Application.Abstractions;
using CursorAgenticWebApi.Infrastructure.Data;
using CursorAgenticWebApi.Infrastructure.Repositories;
using CursorAgenticWebApi.Infrastructure.Security;
using CursorAgenticWebApi.Infrastructure.Seeding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CursorAgenticWebApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<ISqliteConnectionFactory, SqliteConnectionFactory>();
        services.AddScoped<IDatabaseBootstrapper, DatabaseBootstrapper>();
        services.AddScoped<IDataSeeder, DataSeeder>();
        services.AddScoped<IUserRepository, SqliteUserRepository>();
        services.AddScoped<ITaskRepository, SqliteTaskRepository>();
        services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
