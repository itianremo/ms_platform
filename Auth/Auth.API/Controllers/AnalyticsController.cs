using MediatR;
using Microsoft.AspNetCore.Mvc;
using Auth.Application.Features.Auth.Queries.GetAppUserStats;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("app-user-stats")]
    public async Task<IActionResult> GetAppUserStats()
    {
        var query = new GetAppUserStatsQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
