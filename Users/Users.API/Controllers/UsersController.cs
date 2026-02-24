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
        if (result == null) return NoContent();
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

    [HttpGet("profile-ids")]
    public async Task<IActionResult> GetProfileIds()
    {
        var query = new Users.Application.Features.Users.Queries.GetProfileIds.GetProfileIdsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats([FromQuery] Guid? appId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new Users.Application.Features.Users.Queries.GetDashboardStats.GetDashboardStatsQuery(appId, startDate, endDate);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("ReportReasons")]
    public async Task<IActionResult> GetReportReasons()
    {
        var appId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var query = new Users.Application.Queries.GetReportReasons.GetReportReasonsQuery(appId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("Report")]
    public async Task<IActionResult> GetReportReason([FromQuery] Guid reporterId, [FromQuery] Guid reportedId)
    {
        var appId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var query = new Users.Application.Queries.GetReportByUserIds.GetReportByUserIdsQuery(appId, reporterId, reportedId);
        var result = await _mediator.Send(query);
        // Returns the reason and optional comment if exists, otherwise NoContent or Ok(null)
        if (result == null) return Ok(null);
        return Ok(new { reason = result.Reason, comment = result.Comment });
    }

    [HttpPost("Report")]
    public async Task<IActionResult> ReportUser([FromBody] ReportUserRequest request)
    {
        var appId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var command = new Users.Application.Commands.ReportUser.ReportUserCommand(appId, request.ReporterId, request.ReportedId, request.Reason, request.Comment);
        var result = await _mediator.Send(command);
        return result ? Ok() : BadRequest("Failed to submit report.");
    }
}

public record ReportUserRequest(Guid ReporterId, Guid ReportedId, string Reason, string? Comment = null);
