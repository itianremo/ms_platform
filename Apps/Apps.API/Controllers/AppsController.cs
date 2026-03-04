using Apps.Application.Features.Apps.Commands.CreateApp;
using Apps.Application.Features.Apps.Commands.UpdateApp;
using Apps.Application.Features.Apps.Commands.DeactivateApp;
using Apps.Application.Features.Apps.Commands.UpdateExternalAuthConfig;
using Apps.Application.Features.Apps.Commands.DeleteApp;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Apps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> CreateApp([FromBody] CreateAppCommand command)
    {
        var appDto = await _mediator.Send(command);
        return Ok(appDto);
    }

    [HttpGet]
    [Authorize(Policy = "ReadApps")]
    public async Task<IActionResult> GetAllApps()
    {
        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetAllApps.GetAllAppsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ReadApps")]
    public async Task<IActionResult> GetAppById(Guid id)
    {
        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetAppById.GetAppByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> UpdateApp(Guid id, [FromBody] UpdateAppCommand command)
    {
        if (command.Id != Guid.Empty && id != command.Id) return BadRequest();
        
        if (command.Id == Guid.Empty)
        {
            command.Id = id;
        }
        
        var result = await _mediator.Send(command);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] DeactivateAppCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
    [HttpPatch("{id}/external-auth")]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> UpdateExternalAuth(Guid id, [FromBody] UpdateExternalAuthConfigCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "DeleteApps")]
    public async Task<IActionResult> DeleteApp(Guid id)
    {
        var result = await _mediator.Send(new DeleteAppCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/packages")]
    [Authorize(Policy = "ReadApps")]
    public async Task<IActionResult> GetPackages(Guid id, [FromQuery] string? country = null)
    {
        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetPackagesByApp.GetPackagesByAppQuery(id, country));
        return Ok(result);
    }

    [HttpGet("{appId}/users/{userId}/subscriptions")]
    public async Task<IActionResult> GetUserSubscriptions(Guid appId, Guid userId)
    {
        var result = await _mediator.Send(new Apps.Application.Features.Subscriptions.Queries.GetUserSubscriptions.GetUserSubscriptionsQuery(appId, userId));
        return Ok(result);
    }

    [HttpPost("{appId}/users/{userId}/subscriptions")]
    public async Task<IActionResult> GrantSubscription(Guid appId, Guid userId, [FromBody] Apps.Application.Features.Subscriptions.Commands.GrantSubscription.GrantSubscriptionCommand command)
    {
        if (appId != command.AppId || userId != command.UserId) return BadRequest();
        var subId = await _mediator.Send(command);
        return Ok(new { SubscriptionId = subId });
    }

    [HttpPut("{appId}/subscriptions/{id}/status")]
    public async Task<IActionResult> ChangeSubscriptionStatus(Guid appId, Guid id, [FromBody] Apps.Application.Features.Subscriptions.Commands.ChangeStatus.ChangeSubscriptionStatusCommand command)
    {
        if (id != command.SubscriptionId) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
