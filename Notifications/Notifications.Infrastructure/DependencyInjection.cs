using Microsoft.Extensions.DependencyInjection;
using Notifications.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Notifications.Application.Features.Notifications.EventConsumers;

using Notifications.Infrastructure.Services;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailService, MockEmailService>();
        services.AddScoped<ISmsService, SmsService>();

        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => 
                {
                    b.MigrationsAssembly(typeof(NotificationsDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                }));

        services.AddScoped<NotificationsDbContext>();
        services.AddScoped<NotificationsDbInitializer>();
        services.AddScoped<INotificationConfigRepository, NotificationConfigRepository>();
        services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();



        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<Notifications.Application.Features.SendOtp.SendOtpConsumer>();

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

                cfg.ReceiveEndpoint("notification-otp-queue", e =>
                {
                    e.ConfigureConsumer<Notifications.Application.Features.SendOtp.SendOtpConsumer>(context);
                });
            });
        });

        return services;
    }
}
