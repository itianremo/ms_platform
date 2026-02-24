using MediatR;
using Microsoft.AspNetCore.Mvc;
using Auth.Application.Features.Auth.Commands.Maintenance;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaintenanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    public MaintenanceController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpDelete("reset")]
    public async Task<IActionResult> ResetAppData()
    {
        var headerValue = Request.Headers["App-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(headerValue) || !Guid.TryParse(headerValue, out var appId))
        {
            return BadRequest("App-Id header is missing or invalid");
        }
        var secret = Request.Headers["X-Maintenance-Secret"].ToString();
        var expectedSecret = _configuration["Maintenance:Secret"];

        if (string.IsNullOrEmpty(expectedSecret) || secret != expectedSecret)
        {
            return Unauthorized("Invalid Maintenance Secret");
        }

        await _mediator.Send(new ResetAppDataCommand(appId));
        return Ok(new { Message = $"Data for App {appId} has been reset." });
    }
}
