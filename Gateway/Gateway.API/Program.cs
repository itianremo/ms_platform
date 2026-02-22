using Shared.Infrastructure;
using Shared.Infrastructure.Extensions;
using Serilog;

ObservabilityExtensions.ConfigureSerilog(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build());

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddSharedInfrastructure(builder.Configuration);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// Auth Configuration
var jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? "ThisIsSuperSecretKeyForAuthService123!";
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ms-auth";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "ms-platform";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSecret))
    };

    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["token"];
            
            // If empty, check cookie
            if (string.IsNullOrEmpty(accessToken))
            {
                if (context.Request.Cookies.TryGetValue("X-Access-Token", out var cookieToken))
                {
                    accessToken = cookieToken;
                }
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context => 
        {
             var cache = context.HttpContext.RequestServices.GetRequiredService<Shared.Kernel.Interfaces.ICacheService>();
             var sid = context.Principal?.FindFirst("sid")?.Value;
             if (!string.IsNullOrEmpty(sid))
             {
                 var isRevoked = await cache.GetAsync<bool?>($"blacklist:session:{sid}");
                 if (isRevoked == true)
                 {
                     context.Fail("Session revoked");
                 }
             }
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("HealthCheckPolicy", policy => 
        policy.RequireRole("SuperAdmin", "HealthCheck"));
});

builder.Services.AddHealthChecksUI()
    .AddInMemoryStorage();
    
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/auth/swagger/v1/swagger.json", "Auth API");
    c.SwaggerEndpoint("/users/swagger/v1/swagger.json", "Users API");
    c.SwaggerEndpoint("/apps/swagger/v1/swagger.json", "Apps API");
    c.SwaggerEndpoint("/notifications/swagger/v1/swagger.json", "Notifications API");
    c.SwaggerEndpoint("/media/swagger/v1/swagger.json", "Media API");
    c.SwaggerEndpoint("/chat/swagger/v1/swagger.json", "Chat API");
    c.SwaggerEndpoint("/payments/swagger/v1/swagger.json", "Payments API");
    c.SwaggerEndpoint("/audit/swagger/v1/swagger.json", "Audit API");
    c.SwaggerEndpoint("/search/swagger/v1/swagger.json", "Search API");
    c.SwaggerEndpoint("/scheduler/swagger/v1/swagger.json", "Scheduler API");
    c.SwaggerEndpoint("/geo/swagger/v1/swagger.json", "Geo API");
    c.SwaggerEndpoint("/recommendation/swagger/v1/swagger.json", "Recommendation API");
});

app.UseCors("AllowAll");

app.UseRouting();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseSharedHealthChecks();

app.Use(async (context, next) =>
{
    if (context.Request.Path.Value?.StartsWith("/health-dashboard") == true && !context.Request.Path.Value.Contains("."))
    {
        // Token to Cookie Promotion
        if (context.Request.Query.TryGetValue("token", out var tokenValues))
        {
            var tokenStr = tokenValues.ToString();
            if (!string.IsNullOrEmpty(tokenStr))
            {
                context.Response.Cookies.Append("X-Access-Token", tokenStr, new CookieOptions 
                { 
                    HttpOnly = true, 
                    Secure = true, 
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddMinutes(60)
                });
            }
        }

        // Intercept response to inject JS
        var originalBody = context.Response.Body;
        using var newBody = new MemoryStream();
        context.Response.Body = newBody;

        await next();

        context.Response.Body = originalBody;
        newBody.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(newBody).ReadToEndAsync();

        if (context.Response.ContentType?.Contains("text/html") == true)
        {
            responseBody = responseBody.Replace("</body>", "<script src=\"/js/custom.js?v=2\"></script></body>");
            context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(responseBody);
            await context.Response.WriteAsync(responseBody);
        }
        else
        {
            await context.Response.WriteAsync(responseBody);
        }
    }
    else
    {
        await next();
    }
});

app.UseMiddleware<Gateway.API.Middleware.TenantHeaderMiddleware>();
app.UseMiddleware<Gateway.API.Middleware.RedisRateLimitMiddleware>();

app.MapReverseProxy();

app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-dashboard";
    options.ApiPath = "/health-dashboard-api";
    options.AddCustomStylesheet("wwwroot/css/custom.css");
})
.RequireAuthorization("HealthCheckPolicy");

app.Run();
