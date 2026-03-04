using System.Text.Json.Serialization;

namespace Apps.Application.Features.Apps.Commands;

public class SysConfigDto
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "light";
    
    [JsonPropertyName("collapsedmenu")]
    public bool CollapsedMenu { get; set; } = false;

    [JsonPropertyName("defaultcountry")]
    public string DefaultCountry { get; set; } = "US";

    [JsonPropertyName("countrysource")]
    public string CountrySource { get; set; } = "profile"; // "profile" or "location"
}

public class NotificationsDto
{
    [JsonPropertyName("email")]
    public bool Email { get; set; } = true;
    
    [JsonPropertyName("push")]
    public bool Push { get; set; } = true;
    
    [JsonPropertyName("sms")]
    public bool Sms { get; set; } = true;
}
