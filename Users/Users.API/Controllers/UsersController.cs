using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Features.Users.Commands.UpdateProfile;
using Users.Application.Features.Users.Queries.GetProfile;

namespace Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile([FromQuery] Guid userId, [FromQuery] Guid appId)
    {
        var query = new GetProfileQuery(userId, appId);
        var result = await _mediator.Send(query);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }
    [HttpGet("profiles")]
    public async Task<IActionResult> GetProfiles([FromQuery] Guid appId)
    {
        var query = new Users.Application.Features.Users.Queries.GetProfiles.GetProfilesQuery(appId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var query = new Users.Application.Features.Users.Queries.GetDashboardStats.GetDashboardStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
