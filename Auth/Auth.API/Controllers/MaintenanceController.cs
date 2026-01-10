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

    [HttpDelete("reset/{appId}")]
    public async Task<IActionResult> ResetAppData(Guid appId)
    {
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
