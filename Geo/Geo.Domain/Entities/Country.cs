using Shared.Kernel;

namespace Geo.Domain.Entities;

public class Country : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // ISO Code e.g. US, EG
    public string PhoneCode { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
    public ICollection<City> Cities { get; set; } = new List<City>();
}
