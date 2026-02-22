using Shared.Infrastructure.Extensions;
using Notifications.API.Hubs;
using Notifications.Infrastructure;
using Notifications.Application;
using Serilog;

ObservabilityExtensions.ConfigureSerilog(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build());

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR()
    .AddStackExchangeRedis(builder.Configuration["RedisConnectionString"]);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<Notifications.Application.Common.Interfaces.ISignalRService, Notifications.API.Services.SignalRService>();

builder.Services.AddSharedHealthChecks(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient",
        builder => builder
        .WithOrigins("http://localhost:3000", "http://localhost:7032")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<Notifications.Infrastructure.Persistence.NotificationsDbContext>();
        // We can create a temporary initializer or register it in DI.
        // Since it's not registered in DI in Infrastructure DependencyInjection (likely), 
        // we can instantiate manually or register it. 
        // Let's assume we can just instantiate it since it only needs context.
        var initializer = new Notifications.Infrastructure.Persistence.NotificationsDbInitializer(dbContext);
        await initializer.InitializeAsync();
    }
}

app.UseHttpsRedirection();

app.UseCors("AllowClient");

app.UseAuthorization();

app.UseSharedHealthChecks();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
