var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();

app.MapReverseProxy();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-dashboard";
    options.ApiPath = "/health-dashboard-api";
});

app.Run();
