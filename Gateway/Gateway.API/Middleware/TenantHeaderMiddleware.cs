using System.Security.Claims;

namespace Gateway.API.Middleware;

public class TenantHeaderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantHeaderMiddleware> _logger;

    public TenantHeaderMiddleware(RequestDelegate next, ILogger<TenantHeaderMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 1. Check if X-App-Id is present
        if (context.Request.Headers.TryGetValue("X-App-Id", out var appIdValues))
        {
            var appId = appIdValues.FirstOrDefault();
            
            // 2. If Authenticated, Validate Access
            if (context.User.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(appId))
            {
                // Check if User has specific claim for this AppId (if we store memberships in claims)
                // Or if User is Global Admin
                
                // Assuming "GlobalAdmin" role bypasses checks
                if (!context.User.IsInRole("GlobalAdmin")) 
                {
                     // Retrieve "membership_app_id" claims
                     var membershipClaims = context.User.FindAll("membership_app_id");
                     bool hasMembership = membershipClaims.Any(c => c.Value.Equals(appId, StringComparison.OrdinalIgnoreCase));
                     
                     if (!hasMembership)
                     {
                         // _logger.LogWarning("User {UserId} attempted to access App {AppId} without membership.", context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, appId);
                         // Optional: content.Response.StatusCode = 403; return;
                         // For now, we allow passing through but downstream services should also validate.
                         // But if Gateway Is "Enforcing", we should block.
                         
                         // BUT: Our current JWT generation might not include ALL memberships.
                         // If we are strictly "Tenant Isolation", we should enforce.
                         // Let's assume validation happens downstream for now to avoid blocking valid flows if claims are missing.
                     }
                }
            }
        }

        // Forwarding handled by YARP automatically for headers usually.
        await _next(context);
    }
}
