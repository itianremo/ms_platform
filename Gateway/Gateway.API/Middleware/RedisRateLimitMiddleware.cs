using System.Net;
using Shared.Kernel.Interfaces;

namespace Gateway.API.Middleware;

public class RedisRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheService _cache;
    private readonly ILogger<RedisRateLimitMiddleware> _logger;

    // Config: 100 requests per minute
    private const int Limit = 100;
    private const int WindowSeconds = 60;

    public RedisRateLimitMiddleware(RequestDelegate next, ICacheService cache, ILogger<RedisRateLimitMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Identify client: User ID if auth, else IP
        string clientId = context.User.Identity?.IsAuthenticated == true 
            ? context.User.FindFirst("sub")?.Value ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        string key = $"rate_limit:{clientId}";

        var currentCount = await _cache.GetAsync<int?>(key);
        
        if (currentCount.HasValue && currentCount.Value >= Limit)
        {
            _logger.LogWarning($"Rate limit exceeded for {clientId}");
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
            return;
        }

        // Increment
        // Note: Simple increment implementation. Race conditions possible but acceptable for basic rate limiting.
        // ideally use Redis INCR command directly, but ICacheService is generic.
        // For strictness, we'd cast _cache to RedisCacheService or use IDistributedCache directly.
        // We will stick to generic for now.
        
        await _cache.SetAsync(key, (currentCount ?? 0) + 1, TimeSpan.FromSeconds(WindowSeconds));

        await _next(context);
    }
}
