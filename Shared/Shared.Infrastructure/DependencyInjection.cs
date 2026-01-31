using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Caching;
using Shared.Infrastructure.Messaging;
using Shared.Kernel.Interfaces;
using Shared.Infrastructure.Middlewares;

namespace Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "FitIT_";
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // Event Bus (Depends on MassTransit being configured in the app)
        // Event Bus (Depends on MassTransit being configured in the app)
        services.AddScoped<IEventBus, MassTransitEventBus>();

        // Middleware
        services.AddTransient<GlobalExceptionHandler>();

        return services;
    }
}
