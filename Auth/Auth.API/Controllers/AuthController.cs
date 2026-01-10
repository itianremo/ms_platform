using Auth.Application.Features.Auth.Commands.RegisterUser;
using Auth.Application.Features.Auth.Commands.LoginUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { UserId = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand command)
    {
        var token = await _mediator.Send(command);
        SetTokenCookie(token);
        return Ok(new { Token = token });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _mediator.Send(new Auth.Application.Features.Auth.Queries.GetAllUsers.GetAllUsersQuery());
        return Ok(result);
    }

    [HttpPost("reactivate-request")]
    public async Task<IActionResult> RequestReactivation([FromBody] Auth.Application.Features.Auth.Commands.ReactivateUser.RequestReactivationCommand command)
    {
        await _mediator.Send(command);
        return Ok(new { Message = "OTP sent if account exists and is inactive." });
    }

    [HttpPost("reactivate-confirm")]
    public async Task<IActionResult> ConfirmReactivation([FromBody] Auth.Application.Features.Auth.Commands.ReactivateUser.ConfirmReactivationCommand command)
    {
        var success = await _mediator.Send(command);
        if (!success) return BadRequest("Invalid OTP or account status.");
        return Ok(new { Message = "Account reactivated. Please login." });
    }

    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount([FromQuery] string email)
    {
        // TODO: Secure this endpoint with [Authorize] and extract email from claims
        await _mediator.Send(new Auth.Application.Features.Auth.Commands.DeleteUser.DeleteUserCommand(email));
        return Ok(new { Message = "Account soft-deleted." });
    }

    [HttpPost("external-login")]
    public async Task<IActionResult> ExternalLogin([FromBody] Auth.Application.Features.Auth.Commands.ExternalLogin.ExternalLoginCommand command)
    {
        var token = await _mediator.Send(command);
        SetTokenCookie(token);
        return Ok(new { Token = token });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt_token");
        return Ok(new { Message = "Logged out" });
    }

    private void SetTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Requires HTTPS (or localhost)
            SameSite = SameSiteMode.None, // Required for cross-site (if frontend/backend are on different ports/domains)
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("jwt_token", token, cookieOptions);
    }
}
