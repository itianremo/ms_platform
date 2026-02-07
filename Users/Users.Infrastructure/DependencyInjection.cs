using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MassTransit;
using Users.Application.Features.Users.EventConsumers;
using Users.Domain.Repositories;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Repositories;
using Shared.Infrastructure.Caching;
using Shared.Kernel.Interfaces;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddHttpContextAccessor();
        services.AddScoped<Shared.Kernel.Interfaces.ICurrentUserService, Shared.Infrastructure.Services.CurrentUserService>();
        services.AddScoped<Users.Application.Common.Interfaces.IMaintenanceService, Users.Infrastructure.Services.MaintenanceService>();

        // Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? configuration["RedisConnectionString"] ?? "localhost:6379";
            options.InstanceName = "FitIT_";
        });
        services.AddScoped<Shared.Kernel.Interfaces.ICacheService, Shared.Infrastructure.Caching.RedisCacheService>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<UserRegisteredConsumer>();
            x.AddConsumer<UserContactUpdatedConsumer>();

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

                cfg.ReceiveEndpoint("user-contact-updated-queue", e =>
                {
                    e.ConfigureConsumer<UserContactUpdatedConsumer>(context);
                });
            });
        });

        services.AddScoped<UsersDbInitializer>();
        
        // JWT Authentication
        var jwtSettings = new Users.Infrastructure.Authentication.JwtSettings();
        configuration.Bind("JwtSettings", jwtSettings);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
