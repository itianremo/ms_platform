using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Users.Application.Features.Users.EventConsumers;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<Users.Application.Common.Interfaces.IMaintenanceService, Users.Infrastructure.Services.MaintenanceService>();

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

                cfg.ReceiveEndpoint("user-registered-queue", e =>
                {
                    e.ConfigureConsumer<UserRegisteredConsumer>(context);
                });
            });
        });

        return services;
    }
}
