using Shared.Infrastructure.Extensions;
using Media.Infrastructure;
using Media.Application;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseSharedHealthChecks();

app.MapControllers();

app.Run();
