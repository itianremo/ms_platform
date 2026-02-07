using Auth.Application.Features.Auth.Commands.ExternalLogin;
using Auth.Application.Features.Auth.Commands.LinkExternalAccount;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public ExternalAuthController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpGet("login/{provider}")]
    public IActionResult Login(string provider)
    {
        // provider: "Google" or "Microsoft"
        var redirectUrl = Url.Action("Callback", "ExternalAuth", new { provider });
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, provider);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string provider)
    {
        var result = await HttpContext.AuthenticateAsync(provider);
        if (!result.Succeeded)
        {
            return BadRequest("External authentication failed.");
        }

        var claims = result.Principal.Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value 
                    ?? claims.FirstOrDefault(c => c.Type == "email")?.Value;
        
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var phone = claims.FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value 
                    ?? claims.FirstOrDefault(c => c.Type == "phone_number")?.Value; // OIDC standard
        
        // Google/Microsoft usually provide a unique ID in NameIdentifier
        var providerKey = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone)) // Allow if phone exists? 
        {
            // If both missing, we can't do much. BUT originally we checked ProviderKey + Email/Phone logic.
            // If ProviderKey exists, we might find an existing user.
            // But if we need to CREATE, we need Email or Phone.
        }

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone) && string.IsNullOrEmpty(providerKey))
             return BadRequest("Identity missing from external claims.");

        // If providerKey is present, we can try login. If fail, we need email or phone to register.
        // Let Handler handle validation? Handier needs Email/Phone to register.
        
        var command = new ExternalLoginCommand(provider, providerKey ?? "MISSING", email ?? "", name, phone);
        try
        {
            var token = await _mediator.Send(command);
            
            // Redirect to Frontend with Token
            // In production, use a secure cookie or a temporary code exchange. 
            // For this implementation, passing as query param to frontend callback page.
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/auth/callback?token={token}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Login failed: {ex.Message}");
        }
    }

    [HttpGet("link/{provider}")]
    public IActionResult Link(string provider)
    {
        var redirectUrl = Url.Action("CallbackLink", "ExternalAuth", new { provider });
        
        // IMPORTANT: We need to preserve the current UserId.
        // Assuming the user is authenticated via Cookie or Header, HttpContext.User should be set.
        // However, if using Bearer token, the standard Challenge might redirect to provider and lose the context unless dealing with Cookies.
        // If the client App (SPA) triggers this, it's a full page navigation.
        // If state is not lost, we can grab UserId in Callback.
        // Better approach: Pass UserId in state or AuthenticationProperties.
        
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
             // If not authenticated, cannot link.
             // You might want to return Unauthorized, but this is an ActionResult for a redirect info usually.
             // If we return Challenge, the middleware handles it.
             // But we need the UserId in the Callback.
             
             // If we rely on cookie (AuthService has cookies?), simple.
             // If stateless JWT, referencing "User" here implies the request has the token.
             // The CLIENT must navigate to this URL including the Token? e.g. /link/google?access_token=...
             // Or the client does an AJAX call to get the Challenge URL?
             // Standard OAuth Challenge returns a 302.
             
             // For now, assuming Cookie Auth or Token in Query (MVP).
        }
        
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        if (!string.IsNullOrEmpty(userId))
        {
            properties.Items["UserId"] = userId;
        }

        return Challenge(properties, provider);
    }

    [HttpGet("link-callback")]
    public async Task<IActionResult> CallbackLink(string provider)
    {
        // 1. Authenticate with Provider to get their data
        var result = await HttpContext.AuthenticateAsync(provider);
        if (!result.Succeeded)
        {
            return BadRequest("External authentication failed.");
        }

        // 2. Retrieve Preserved UserId
        // Note: Check result.Properties.Items first. 
        // If using Cookie Auth, HttpContext.User might still be populated if the cookie persisted?
        // But "AuthenticateAsync(provider)" returns the PROVIDER identity, not the App identity.
        // Use the Properties we sent.
        
        string? userId = null;
        if (result.Properties != null && result.Properties.Items.ContainsKey("UserId"))
        {
            userId = result.Properties.Items["UserId"];
        }

        // 3. Get Provider Info
        var providerKey = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(providerKey))
        {
            return BadRequest("Link failed: Missing User Context or Provider Key.");
        }

        // 4. Link
        var command = new LinkExternalAccountCommand(Guid.Parse(userId), provider, providerKey, name);
        var success = await _mediator.Send(command);

        var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
        if (success)
        {
            return Redirect($"{frontendUrl}/profile?status=linked");
        }
        else
        {
            return Redirect($"{frontendUrl}/profile?status=link_failed");
        }
    }
    [HttpDelete("users/{userId}/links/{provider}")]
    public async Task<IActionResult> Unlink(Guid userId, string provider)
    {
        var command = new Auth.Application.Features.Auth.Commands.UnlinkExternalAccount.UnlinkExternalAccountCommand(userId, provider);
        var success = await _mediator.Send(command);
        if (success)
            return Ok();
        return BadRequest("Failed to unlink or link not found.");
    }
}
