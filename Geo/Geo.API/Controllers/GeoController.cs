using Geo.API.DTOs;
using Geo.Domain.Entities;
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
        return Ok(results);
    }

    [HttpPost("location")]
    public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationRequest request)
    {
        var existing = await _repository.GetByUserIdAsync(request.UserId, request.AppId);
        
        var point = new NetTopologySuite.Geometries.Point(new NetTopologySuite.Geometries.Coordinate(request.Longitude, request.Latitude)) 
        { 
            SRID = 4326 
        };

        if (existing != null)
        {
            existing.Location = point;
            existing.LastUpdated = DateTime.UtcNow;
            await _repository.UpdateAsync(existing);
        }
        else
        {
            var newLocation = new GeoLocation
            {
                UserId = request.UserId,
                AppId = request.AppId,
                Location = point,
                LastUpdated = DateTime.UtcNow
            };
            await _repository.AddAsync(newLocation);
        }

        return Ok();
    }
}
