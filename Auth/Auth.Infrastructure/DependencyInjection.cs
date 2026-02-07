using Auth.Application.Common.Interfaces;
using Shared.Kernel;
using Auth.Domain.Repositories;
using Auth.Infrastructure.Persistence;
using Auth.Infrastructure.Repositories;
using Auth.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<Shared.Kernel.Interfaces.ICurrentUserService, Shared.Infrastructure.Services.CurrentUserService>();
        services.AddScoped<Shared.Infrastructure.Persistence.Interceptors.AuditableEntitySaveChangesInterceptor>();

        services.AddDbContext<AuthDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<Shared.Infrastructure.Persistence.Interceptors.AuditableEntitySaveChangesInterceptor>();
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null))
                   .AddInterceptors(interceptor);
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IMaintenanceService, MaintenanceService>();
        services.AddScoped<Shared.Kernel.IRepository<Auth.Domain.Entities.UserOtp>, Auth.Infrastructure.Repositories.UserOtpRepository>();
        services.AddScoped<Shared.Kernel.IRepository<Auth.Domain.Entities.UserAppMembership>, Auth.Infrastructure.Repositories.Repository<Auth.Domain.Entities.UserAppMembership>>();
        services.AddScoped<Shared.Kernel.IRepository<Auth.Domain.Entities.Role>, Auth.Infrastructure.Repositories.Repository<Auth.Domain.Entities.Role>>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        
        // Caching
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "Auth_";
        });
        services.AddScoped<Shared.Kernel.Interfaces.ICacheService, Shared.Infrastructure.Caching.RedisCacheService>();



        
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
                {
                    h.Username(configuration["RabbitMq:Username"] ?? "guest");
                    h.Password(configuration["RabbitMq:Password"] ?? "guest");
                });
            });
        });

        services.AddScoped<AuthDbInitializer>();
        
        // Add Authentication & JWT Bearer
        var jwtSettings = new JwtSettings();
        configuration.Bind("JwtSettings", jwtSettings);
        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // For dev/docker
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
        
        if (!string.IsNullOrEmpty(configuration["Authentication:Google:ClientId"]))
        {
            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            });
        }

        if (!string.IsNullOrEmpty(configuration["Authentication:Microsoft:ClientId"]))
        {
            services.AddAuthentication().AddMicrosoftAccount(options =>
            {
                options.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
            });
        }

        return services;
    }
}
