using Shared.Infrastructure.Extensions;
using Chat.Infrastructure;
using Chat.Application;
using Chat.API.Hubs;
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
builder.Services.AddSignalR();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddSharedHealthChecks(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSharedHealthChecks();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
