using Shared.Infrastructure.Extensions;
using Apps.Infrastructure;
using Apps.Infrastructure;
using Apps.Application;
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
builder.Services.AddHttpClient();

// Add Authentication
var jwtSettings = new Apps.API.JwtSettings();
builder.Configuration.Bind("JwtSettings", jwtSettings);
builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(jwtSettings));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; 
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ReadApps", policy => 
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type.Equals("permission", StringComparison.OrdinalIgnoreCase) && 
                (c.Value == "AccessAll" || c.Value == "AssignApps" || c.Value == "EditApps" || c.Value == "ManageApps" || c.Value == "FreeSubscription" || c.Value == "VipSubscription"))
        ));

    options.AddPolicy("WriteApps", policy => 
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type.Equals("permission", StringComparison.OrdinalIgnoreCase) && 
                (c.Value == "AccessAll" || c.Value == "EditApps" || c.Value == "ManageApps"))
        ));

    options.AddPolicy("DeleteApps", policy => 
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type.Equals("permission", StringComparison.OrdinalIgnoreCase) && 
                (c.Value == "AccessAll" || c.Value == "DeleteApps" || c.Value == "ManageApps"))
        ));
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

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Apps.Infrastructure.Persistence.AppsDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Apps.Infrastructure.Persistence.AppsDbInitializer>>();
    await Apps.Infrastructure.Persistence.AppsDbInitializer.InitializeAsync(context, logger);
}

// Configure the HTTP request pipeline.
app.UseSwagger();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseSharedHealthChecks();

app.MapControllers();

app.Run();
