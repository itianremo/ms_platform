using NetTopologySuite.Geometries;
using Shared.Kernel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Geo.Domain.Entities;

public class GeoLocation : Entity
{
    public int UserId { get; set; }
    public int AppId { get; set; }

    [Column(TypeName = "geography(Point, 4326)")]
    public Point Location { get; set; }

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Concurrency Token? Not strictly needed for simple location
}
