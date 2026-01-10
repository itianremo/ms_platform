using Audit.Domain.Entities;
using Audit.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Audit.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _repository;

    public AuditController(IAuditRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLog>>> GetLogs([FromQuery] int? appId, [FromQuery] int? userId)
    {
        if (appId.HasValue && userId.HasValue)
        {
            return await _repository.ListAsync(x => x.AppId == appId && x.UserId == userId);
        }
        else if (appId.HasValue)
        {
            return await _repository.ListAsync(x => x.AppId == appId);
        }
        
        return await _repository.ListAsync(); // Should be paged in real app
    }
}
