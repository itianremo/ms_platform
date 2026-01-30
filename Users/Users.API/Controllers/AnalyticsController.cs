using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Features.Users.Queries.GetDashboardStats;

namespace Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "GlobalAdmin")] // Strict security
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardStats()
    {
        // Aggregate high-level stats
        var query = new GetDashboardStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
