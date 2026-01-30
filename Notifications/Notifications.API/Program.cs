using Notifications.Application;
using Notifications.Infrastructure;
using Notifications.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddRabbitMQ(rabbitConnectionString: $"amqp://{builder.Configuration["RabbitMq:Username"]}:{builder.Configuration["RabbitMq:Password"]}@{builder.Configuration["RabbitMq:Host"]}/");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

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

app.UseAuthorization();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
