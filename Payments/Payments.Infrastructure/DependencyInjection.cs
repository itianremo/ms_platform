using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Domain.Interfaces;
using Payments.Domain.Repositories;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Repositories;
using Payments.Infrastructure.Gateways;
using MassTransit;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PaymentsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
        services.AddScoped<IAppPaymentProviderRepository, AppPaymentProviderRepository>();
        
        // Gateways
        services.AddScoped<MockGateway>();
        services.AddScoped<StripeGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<Payments.Application.Features.Subscriptions.Commands.ProcessRenewals.ProcessSubscriptionRenewalsConsumer>();
            
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"], "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"]);
                    h.Password(configuration["RabbitMq:Password"]);
                });
                
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<PaymentsDbInitializer>();
        return services;
    }
}
