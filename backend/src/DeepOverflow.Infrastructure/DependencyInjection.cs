using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Interfaces;
using DeepOverflow.Infrastructure.Persistence;
using DeepOverflow.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeepOverflow.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Cross-cutting services
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        // External integrations (safe defaults = no-op)
        services.AddSingleton<ISearchService, NoOpSearchService>();
        services.AddSingleton<IAIService, NoOpAIService>();
        services.AddSingleton<ICacheService, NoOpCacheService>();
        services.AddSingleton<INotificationService, NoOpNotificationService>();

        return services;
    }
}

