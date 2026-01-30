using Auth.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Auth.API.Filters;

public class SessionValidationFilter : IAsyncActionFilter
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SessionValidationFilter> _logger;

    public SessionValidationFilter(IUserRepository userRepository, ILogger<SessionValidationFilter> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Check if user is authenticated
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        // 2. Extract Session ID (sid) claim
        var sidClaim = context.HttpContext.User.FindFirst("sid");
        if (sidClaim == null || !Guid.TryParse(sidClaim.Value, out var sessionId))
        {
            // If authenticated but no SID (legacy token?), we might allow or block.
            // For now, let's allow, assuming legacy tokens will expire naturally.
            // OR enforce strictness if we want to force logout on all old tokens.
            // Let's log warning and proceed to avoid breaking existing valid (but old) sessions temporarily.
            // _logger.LogWarning("Authenticated request missing 'sid' claim.");
            await next();
            return;
        }

        // 3. Validate Session against DB
        var session = await _userRepository.GetSessionByIdAsync(sessionId);

        // 4. Check Revocation
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found in DB. Treating as revoked.", sessionId);
            context.Result = new UnauthorizedObjectResult("Session not found.");
            return;
        }

        if (session.IsRevoked)
        {
            _logger.LogWarning("Session {SessionId} is REVOKED. Denying access.", sessionId);
            context.Result = new UnauthorizedObjectResult("Session is revoked.");
            return;
        }

        // Check Expiry (Double check, although JWT expiry handles stateless check)
        if (session.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Session {SessionId} is EXPIRED. Denying access.", sessionId);
            context.Result = new UnauthorizedObjectResult("Session expired.");
            return;
        }

        // Success
        await next();
    }
}
