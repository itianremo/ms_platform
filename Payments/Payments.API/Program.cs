using Shared.Infrastructure.Extensions;
using Payments.Infrastructure;
using Payments.Application;
using Payments.Infrastructure.Persistence; // For PaymentsDbInitializer
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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSharedHealthChecks(builder.Configuration);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["ConnectionStrings:Redis"] ?? "localhost:6379";
    options.InstanceName = "Payments:";
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();

// Initialize Database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<PaymentsDbInitializer>();
    await initializer.InitializeAsync();
}

app.UseSharedHealthChecks();

app.MapControllers();

app.Run();
