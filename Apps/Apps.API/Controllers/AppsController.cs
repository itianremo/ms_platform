using Apps.Application.Features.Apps.Commands.CreateApp;
using Apps.Application.Features.Apps.Commands.UpdateApp;
using Apps.Application.Features.Apps.Commands.DeactivateApp;
using Apps.Application.Features.Apps.Commands.UpdateExternalAuthConfig;
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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApp(Guid id, [FromBody] UpdateAppCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] DeactivateAppCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
    [HttpPatch("{id}/external-auth")]
    public async Task<IActionResult> UpdateExternalAuth(Guid id, [FromBody] UpdateExternalAuthConfigCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
