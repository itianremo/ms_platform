using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Serilog;

namespace Shared.Infrastructure.Extensions;

public static class ObservabilityExtensions
{
    public static IServiceCollection AddSharedHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        // SQL Server vs Postgres based on connection string content
        var defaultConnection = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(defaultConnection))
        {
            if (defaultConnection.Contains("5432") || defaultConnection.Contains("postgres") || defaultConnection.Contains("Host="))
            {
                 healthChecks.AddNpgSql(defaultConnection, name: "PostgreSQL");
            }
            else
            {
                 // Default to SQL Server
                 healthChecks.AddSqlServer(defaultConnection, name: "SQL Server");
            }
        }

        var redisConnection = configuration["RedisConnectionString"]; // Using Env Var name directly or ConnectionStrings
        if (!string.IsNullOrEmpty(redisConnection))
        {
            // Parse standard redis connection string if needed, or just pass it
            healthChecks.AddRedis(redisConnection, name: "Redis");
        }

        // RabbitMQ
        var rabbitHost = configuration["RabbitMq:Host"];
        if (!string.IsNullOrEmpty(rabbitHost))
        {
            // Basic check, might need credentials if set
            // Assumes standard port 5672
            // Construct uri: amqp://guest:guest@host:5672/
            var user = configuration["RabbitMq:Username"] ?? "guest";
            var pass = configuration["RabbitMq:Password"] ?? "guest";
            var uri = $"amqp://{user}:{pass}@{rabbitHost}:5672/";
            healthChecks.AddRabbitMQ(rabbitConnectionString: uri, name: "RabbitMQ");
        }

        // Mongo (if connection string exists)
        var mongoConnection = configuration.GetConnectionString("MongoConnection");
        if (!string.IsNullOrEmpty(mongoConnection))
        {
            healthChecks.AddMongoDb(mongoConnection, name: "MongoDB");
        }



        return services;
    }

    public static WebApplication UseSharedHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return app;
    }

    public static void ConfigureSerilog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name)
            .WriteTo.Console()
            .WriteTo.Seq(configuration["SeqServerUrl"] ?? "http://localhost:5341")
            .CreateLogger();
    }
}
