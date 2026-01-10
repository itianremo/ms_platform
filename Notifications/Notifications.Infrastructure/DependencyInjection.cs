using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Notifications.Application.Features.Notifications.EventConsumers;
using Notifications.Infrastructure.Services;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });

                cfg.ReceiveEndpoint("notification-welcome-queue", e =>
                {
                    e.ConfigureConsumer<UserRegisteredConsumer>(context);
                });
            });
        });

        return services;
    }
}
