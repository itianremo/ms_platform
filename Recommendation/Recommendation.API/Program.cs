using Shared.Infrastructure.Extensions;
using Recommendation.Infrastructure;
using Recommendation.Application;
using Recommendation.Domain.Entities;
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
builder.Services.AddInfrastructure();

builder.Services.AddSharedHealthChecks(builder.Configuration);

var app = builder.Build();

// Train Model on Startup (Mock Data)
var engine = app.Services.GetRequiredService<IRecommendationEngine>();
var trainingData = new List<RecommendationData>
{
    new() { UserId = 1, ItemId = 10, Label = 1 },
    new() { UserId = 1, ItemId = 11, Label = 0 }, // Dislike
    new() { UserId = 2, ItemId = 10, Label = 1 },
    new() { UserId = 2, ItemId = 12, Label = 1 },
    new() { UserId = 3, ItemId = 11, Label = 1 },
};
engine.Train(trainingData);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseSharedHealthChecks();

app.MapControllers();

app.Run();
