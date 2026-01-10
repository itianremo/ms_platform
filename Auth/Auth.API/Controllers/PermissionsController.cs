using Auth.Application.Features.Auth.Queries.CheckPermission;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission([FromQuery] Guid userId, [FromQuery] Guid appId, [FromQuery] string permission)
    {
        var query = new CheckPermissionQuery(userId, appId, permission);
        var result = await _mediator.Send(query);
        return Ok(new { Allowed = result });
    }
}
