using MediatR;
using Microsoft.AspNetCore.Mvc;
using Users.Application.Features.Users.Queries.GetAuthProfile;

namespace Users.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InternalController : ControllerBase
{
    private readonly IMediator _mediator;

    public InternalController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Explicitly for Auth.API to verify RoleId during login flow
    [HttpGet("auth-profile")]
    public async Task<IActionResult> GetAuthProfile([FromQuery] Guid userId, [FromQuery] Guid appId)
    {
        var query = new GetAuthProfileQuery(userId, appId);
        var result = await _mediator.Send(query);
        
        if (result == null) return NotFound();
        
        return Ok(result);
    }
}
