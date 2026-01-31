using Shared.Infrastructure.Extensions;
using Hangfire;
using MassTransit;
using Hangfire.SqlServer;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
ObservabilityExtensions.ConfigureSerilog(builder.Configuration);

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ensure DB exists
try 
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        Scheduler.API.Data.DatabaseInitializer.EnsureDatabaseCreated(connectionString);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Check DB failed: {ex.Message}");
    // Continue, maybe it exists or it's a transient error the retry policy will handle? 
    // But creation failure usually is fatal for start.
}

// Hangfire Client
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Hangfire Server
builder.Services.AddHangfireServer();

// Health Checks
builder.Services.AddSharedHealthChecks(builder.Configuration);

// MassTransit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("rabbitmq", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.UseSharedHealthChecks();

app.UseHangfireDashboard(); // /hangfire

// Define Recurring Jobs
app.MapControllers();

// Register jobs
RecurringJob.AddOrUpdate("heartbeat", () => Console.WriteLine($"Heartbeat: {DateTime.UtcNow}"), Cron.Minutely);
RecurringJob.AddOrUpdate<Scheduler.API.Jobs.SubscriptionRenewalJob>("subscription-renewal", job => job.ExecuteAsync(), Cron.Daily);

app.Run();
