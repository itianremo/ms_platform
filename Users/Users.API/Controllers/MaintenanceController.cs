using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Features.Users.Commands.Maintenance;

namespace Users.API.Controllers;

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
        return Ok(new { Message = $"User Profiles for App {appId} have been reset." });
    }
}
