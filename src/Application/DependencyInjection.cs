using CursorAgenticWebApi.Application.Auth;
using CursorAgenticWebApi.Application.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace CursorAgenticWebApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<UserRegistrationService>();
        services.AddScoped<UserLoginService>();
        services.AddScoped<UserProfileService>();
        services.AddScoped<TaskApplicationService>();
        return services;
    }
}
