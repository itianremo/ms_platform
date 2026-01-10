using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Search.Domain.Repositories;
using Search.Infrastructure.Messaging;
using Search.Infrastructure.Persistence;
using Search.Infrastructure.Repositories;

namespace Search.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SearchDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISearchRepository, SearchRepository>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserProfileConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]);
                    h.Password(configuration["RabbitMq:Password"]);
                });

                cfg.ReceiveEndpoint("search-service-queue", e =>
                {
                    e.ConfigureConsumer<UserProfileConsumer>(context);
                });
            });
        });

        return services;
    }
}
