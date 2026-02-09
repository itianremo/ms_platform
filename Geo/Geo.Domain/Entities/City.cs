using Shared.Kernel;

namespace Geo.Domain.Entities;

public class City : Entity
{
    public string Name { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public Country? Country { get; set; }
    public bool IsActive { get; set; } = true;
}
