using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Auth.Application.Features.Auth.Queries.GetAppUserStats;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "DashboardRead")]
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
        // Extract App-Id from header
        var headerValue = Request.Headers["App-Id"].FirstOrDefault();
        Guid? filterAppId = null;

        if (!string.IsNullOrEmpty(headerValue))
        {
            if (Guid.TryParse(headerValue, out var parsedGuid))
            {
                filterAppId = parsedGuid;
            }
            else
            {
                throw new global::Auth.Domain.Exceptions.NotFoundException("App not found");
            }
        }

        var query = new GetAppUserStatsQuery { FilterAppId = filterAppId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
