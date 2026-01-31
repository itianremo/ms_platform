using Apps.Domain.Repositories;
using Apps.Infrastructure.Persistence;
using Apps.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Apps.Application.Features.Subscriptions.Consumers.PaymentSucceededConsumer>();
            
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
