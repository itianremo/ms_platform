using Apps.Application.Features.Apps.Commands.CreateApp;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Apps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateApp([FromBody] CreateAppCommand command)
    {
        var appId = await _mediator.Send(command);
        return CreatedAtAction(nameof(CreateApp), new { id = appId }, new { Id = appId });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllApps()
    {
        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetAllApps.GetAllAppsQuery());
        return Ok(result);
    }
}
