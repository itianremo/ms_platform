namespace Geo.API.DTOs;

public class UpdateLocationRequest
{
    public int UserId { get; set; }
    public int AppId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
