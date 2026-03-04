using System.Text.Json.Serialization;

namespace Apps.Application.Features.Apps.Commands;

public class SysConfigDto
{
    [JsonPropertyName("theme")]
    public string Theme { get; set; } = "light";
    
    [JsonPropertyName("collapsedmenu")]
    public bool CollapsedMenu { get; set; } = false;
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
