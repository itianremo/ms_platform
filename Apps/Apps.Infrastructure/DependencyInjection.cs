using Apps.Domain.Repositories;
using Apps.Infrastructure.Persistence;
using Apps.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure.Caching;
using Shared.Kernel.Interfaces;

namespace Apps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)));

        services.AddScoped<IAppRepository, AppRepository>();
        services.AddScoped<ISubscriptionPackageRepository, SubscriptionPackageRepository>();
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();

        // Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? configuration["RedisConnectionString"] ?? "localhost:6379";
            options.InstanceName = "FitIT_";
        });
        services.AddScoped<Shared.Kernel.Interfaces.ICacheService, Shared.Infrastructure.Caching.RedisCacheService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Apps.Application.Features.Subscriptions.Consumers.PaymentSucceededConsumer>();
            x.AddConsumer<Apps.Application.Features.AdminSubscription.UserRoleAssignedConsumer>();
            x.AddConsumer<Apps.Application.Features.AdminSubscription.UserRoleRemovedConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });
            });
        });

        return services;
    }
}
