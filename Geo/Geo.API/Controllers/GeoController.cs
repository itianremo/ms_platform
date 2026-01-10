using Geo.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Geo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GeoController : ControllerBase
{
    private readonly IGeoRepository _repository;

    public GeoController(IGeoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby([FromQuery] int appId, [FromQuery] double lat, [FromQuery] double lon, [FromQuery] double radiusKm = 10)
    {
        var results = await _repository.GetNearbyAsync(appId, lat, lon, radiusKm);
        
        // Convert to DTO to avoid exposing NTS types directly if needed, 
        // but System.Text.Json might need config for NTS.
        // For now returning as is to test.
        return Ok(results);
    }
}
