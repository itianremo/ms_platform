using Auth.Application.Features.Auth.Commands.RegisterUser;
using Auth.Application.Features.Auth.Commands.LogoutUser;
using Auth.Application.Features.Auth.Commands.LoginUser;
using MediatR;
using global::Auth.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Auth.Domain.Entities;

using Auth.Application.Features.Auth.Commands.RequestOtp;
using Auth.Application.Features.Auth.Commands.VerifyOtp;
using Auth.Application.Features.Auth.Commands.RefreshToken;
using Auth.Application.Features.Auth.DTOs;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ServiceFilter(typeof(Auth.API.Filters.SessionValidationFilter))]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { userId = result });
    }

    [HttpPost("login")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        try 
        {
            // Extract AppId from Header
            var headerValueAuth = Request.Headers["App-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValueAuth) && Guid.TryParse(headerValueAuth, out var appIdAuth))
            {
                 command = command with { AppId = appIdAuth };
            }

            // Capture IP and UserAgent
            var ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers["User-Agent"].ToString();
            command = command with { IpAddress = ip, UserAgent = ua };

            var authResponse = await _mediator.Send(command);
            
            // Set Access Token Cookie (for health check / fallback)
            SetTokenCookie(authResponse.AccessToken, "jwt_token", authResponse.ExpiresIn / 60); 

            // Set Refresh Token Cookie (HttpOnly, Secure)
            SetTokenCookie(authResponse.RefreshToken, "refresh_token", 7 * 24 * 60); 
            
            return Ok(authResponse);
        }
        catch (RequiresVerificationException ex)
        {
            return StatusCode(403, new { Error = "RequiresVerification", Status = ex.RequiredStatus.ToString(), Phone = ex.Phone });
        }
        catch (RequiresAdminApprovalException)
        {
            return StatusCode(403, new { Error = "RequiresAdminApproval", Message = "Account pending admin approval." });
        }
        }

    [HttpPost("otp/request")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "OTP sent." });
    }

    [HttpPost("otp/verify")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
    {
        var result = await _mediator.Send(command);
        
        // Similar to Login, set cookies
        SetTokenCookie(result.AccessToken, "jwt_token", result.ExpiresIn / 60);
        SetTokenCookie(result.RefreshToken, "refresh_token", 7 * 24 * 60);

        return Ok(result);
    }

    [HttpPost("password/forgot")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ForgotPassword([FromBody] Auth.Application.Features.Auth.Commands.ForgotPassword.ForgotPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "If an account exists, a reset code has been sent." });
    }

    [HttpPost("password/reset")]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ResetPassword([FromBody] Auth.Application.Features.Auth.Commands.ResetPassword.ResetPasswordCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "Password reset successfully." });
    }

    [HttpPost("reactivate/initiate")]
    public async Task<IActionResult> InitiateReactivation([FromBody] Auth.Application.Features.Auth.Commands.ReactivateUser.InitiateReactivationCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "If the account exists and matches, a verification code has been sent." });
    }

    [HttpPost("reactivate/verify")]
    public async Task<IActionResult> VerifyReactivation([FromBody] Auth.Application.Features.Auth.Commands.ReactivateUser.VerifyReactivationCommand command)
    {
        var success = await _mediator.Send(command);
        if (!success) return BadRequest(new { Message = "Invalid code or failed to reactivate." }); // Should throw usually
        return Ok(new { Message = "Account reactivated successfully. Please login." });
    }


    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        // Get Refresh Token from Cookie
        if (!Request.Cookies.TryGetValue("refresh_token", out var refreshToken))
        {
             return Unauthorized(new { Message = "No refresh token provided" });
        }
        
        // AppId optional from header? Or embedded in session? Session has it. 
        // But GenerateAccessToken might need context if user has multi app roles.
        // Let's grab AppId from header if present.
        Guid? appId = null;
        var headerValueReq = Request.Headers["App-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerValueReq) && Guid.TryParse(headerValueReq, out var parsedAppId))
        {
             appId = parsedAppId;
        }

        try 
        {
            var command = new Auth.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand(refreshToken, appId);
            var result = await _mediator.Send(command);
            
            // Renew Cookies
            SetTokenCookie(result.AccessToken, "jwt_token", result.ExpiresIn / 60);
            
            // If we decided to rotate refresh tokens, we would set the new one here.
            // For now, we are returning the same refresh token or a new one if implementation changed.
            // If we assume rotation:
            // SetTokenCookie(result.RefreshToken, "refresh_token", 7 * 24 * 60);
            
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    [HttpGet("sessions")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> GetSessions()
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        var sessions = await _mediator.Send(new Auth.Application.Features.Auth.Queries.GetUserSessions.GetUserSessionsQuery(userId));
        return Ok(sessions);
    }
    
    [HttpPut("users/{id}/status")]
    // [Authorize(Roles = "SuperAdmin,UsersManager")] 
    public async Task<IActionResult> UpdateUserStatus(Guid id, [FromBody] UpdateUserStatusRequest request)
    {
        await _mediator.Send(new Auth.Application.Features.Auth.Commands.Maintenance.UpdateUserStatus.UpdateUserStatusCommand(id, (GlobalUserStatus)request.Status));
        return Ok();
    }

    [HttpPut("users/{id}/verify")]
    public async Task<IActionResult> UpdateUserVerification(Guid id, [FromBody] UpdateUserVerificationRequest request)
    {
        await _mediator.Send(new Auth.Application.Features.Auth.Commands.Maintenance.UpdateUserVerification.UpdateUserVerificationCommand(id, request.Type, request.Verified));
        return Ok();
    }

    // App Membership Endpoints
    [HttpPost("users/{id}/apps")]
    public async Task<IActionResult> AddUserToApp(Guid id, [FromBody] AddUserToAppRequest request)
    {
        var appId = GetAppIdFromHeader();
        await _mediator.Send(new Auth.Application.Features.Auth.Commands.Maintenance.ManageAppMembership.AddUserToAppCommand(id, appId, request.RoleId));
        return Ok();
    }

    [HttpDelete("users/{id}/apps")]
    public async Task<IActionResult> RemoveUserFromApp(Guid id)
    {
        var appId = GetAppIdFromHeader();
        await _mediator.Send(new Auth.Application.Features.Auth.Commands.Maintenance.ManageAppMembership.RemoveUserFromAppCommand(id, appId));
        return Ok();
    }

    [HttpPut("users/{id}/apps/status")]
    public async Task<IActionResult> UpdateAppStatus(Guid id, [FromBody] UpdateAppStatusRequest request)
    {
         var appId = GetAppIdFromHeader();
         await _mediator.Send(new Auth.Application.Features.Auth.Commands.Maintenance.ManageAppMembership.UpdateAppMembershipStatusCommand(id, appId, request.Status));
         return Ok();
    }
    
    // Role Management
    [HttpPut("users/{id}/apps/role")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleRequest request)
    {
        var appId = GetAppIdFromHeader();
        await _mediator.Send(new Auth.Application.Features.Auth.Commands.Maintenance.ManageUserRoles.AssignRoleCommand(id, appId, request.RoleName));
        return Ok();
    }

    public record UpdateUserStatusRequest(int Status);
    public record UpdateUserVerificationRequest(string Type, bool Verified);
    public record AddUserToAppRequest(Guid? RoleId);
    public record UpdateAppStatusRequest(int Status);
    public record AssignRoleRequest(string RoleName);

    [HttpDelete("sessions/{sessionId}")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> RevokeSession(Guid sessionId)
    {
        var userId = GetUserIdFromClaims();
        if (userId == Guid.Empty) return Unauthorized();

        var success = await _mediator.Send(new Auth.Application.Features.Auth.Commands.RevokeSession.RevokeSessionCommand(userId, sessionId));
        if (!success) return NotFound("Session not found or already revoked.");
        
        return Ok(new { Message = "Session revoked." });
    }

    // ... Helper to get ID ...
    private Guid GetUserIdFromClaims()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier); // "sub" is mapped to NameIdentifier by default
        if (claim == null) return Guid.Empty;
        return Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var appId = GetOptionalAppIdFromHeader();
        var result = await _mediator.Send(new Auth.Application.Features.Auth.Queries.GetAllUsers.GetAllUsersQuery(appId));
        return Ok(result);
    }

    // ... existing ...

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // cleanup cookies
        Response.Cookies.Delete("jwt_token");
        Response.Cookies.Delete("refresh_token");

        var userId = GetUserIdFromClaims();
        if (userId != Guid.Empty)
        {
            // Try to get SessionId from claims
            var sessionIdClaim = User.FindFirst("sid");
            if (sessionIdClaim != null && Guid.TryParse(sessionIdClaim.Value, out var sessionId))
            {
                 await _mediator.Send(new LogoutUserCommand(userId, sessionId));
            }
        }

        return Ok(new { Message = "Logged out" });
    }

    private void SetTokenCookie(string token, string name, int expiryMinutes)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, 
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes)
        };
        Response.Cookies.Append(name, token, cookieOptions);
    }
    private Guid GetAppIdFromHeader()
    {
        var headerValue = Request.Headers["App-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerValue) && Guid.TryParse(headerValue, out var parsedGuid))
        {
            return parsedGuid;
        }
        throw new global::Auth.Domain.Exceptions.NotFoundException("App-Id header is missing or invalid");
    }

    private Guid? GetOptionalAppIdFromHeader()
    {
        var headerValue = Request.Headers["App-Id"].FirstOrDefault();
        if (!string.IsNullOrEmpty(headerValue) && Guid.TryParse(headerValue, out var parsedGuid))
        {
            return parsedGuid;
        }
        return null;
    }
}
