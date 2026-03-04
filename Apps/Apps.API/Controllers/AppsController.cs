using Apps.Application.Features.Apps.Commands.CreateApp;
using Apps.Application.Features.Apps.Commands.UpdateApp;
using Apps.Application.Features.Apps.Commands.DeactivateApp;
using Apps.Application.Features.Apps.Commands.UpdateExternalAuthConfig;
using Apps.Application.Features.Apps.Commands.DeleteApp;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Apps.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHttpClientFactory _httpClientFactory;

    public AppsController(IMediator mediator, IHttpClientFactory httpClientFactory)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> CreateApp([FromBody] CreateAppCommand command)
    {
        var appDto = await _mediator.Send(command);
        return Ok(appDto);
    }

    [HttpGet]
    [Authorize(Policy = "ReadApps")]
    public async Task<IActionResult> GetAllApps()
    {
        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetAllApps.GetAllAppsQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ReadApps")]
    public async Task<IActionResult> GetAppById(Guid id)
    {
        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetAppById.GetAppByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> UpdateApp(Guid id, [FromBody] UpdateAppCommand command)
    {
        if (command.Id != Guid.Empty && id != command.Id) return BadRequest();
        
        if (command.Id == Guid.Empty)
        {
            command.Id = id;
        }
        
        var result = await _mediator.Send(command);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> ToggleStatus(Guid id, [FromBody] DeactivateAppCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
    [HttpPatch("{id}/external-auth")]
    [Authorize(Policy = "WriteApps")]
    public async Task<IActionResult> UpdateExternalAuth(Guid id, [FromBody] UpdateExternalAuthConfigCommand command)
    {
        if (id != command.Id) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "DeleteApps")]
    public async Task<IActionResult> DeleteApp(Guid id)
    {
        var result = await _mediator.Send(new DeleteAppCommand(id));
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/packages")]
    [Authorize(Policy = "ReadApps")]
    public async Task<IActionResult> GetPackages(Guid id, [FromQuery] string? country = null)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier || c.Type == "sub")?.Value;
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var parsedUserId))
        {
            userId = parsedUserId;
        }
        string? authToken = Request.Headers.Authorization.FirstOrDefault();

        var appDto = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetAppById.GetAppByIdQuery(id));
        if (appDto == null) return NotFound();
        
        string defaultCountry = "US";
        string countrySource = "profile";

        if (appDto.DynamicData != null && appDto.DynamicData.TryGetValue("sysConfig", out var sysConfigElement) && sysConfigElement.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            if (sysConfigElement.TryGetProperty("defaultcountry", out var dcProp) || sysConfigElement.TryGetProperty("defaultCountry", out dcProp))
            {
                var val = dcProp.GetString();
                if (!string.IsNullOrEmpty(val)) defaultCountry = val;
            }
            if (sysConfigElement.TryGetProperty("countrysource", out var csProp) || sysConfigElement.TryGetProperty("countrySource", out csProp))
            {
                var val = csProp.GetString();
                if (!string.IsNullOrEmpty(val)) countrySource = val.ToLower();
            }
        }

        if (string.IsNullOrEmpty(country))
        {
            if (countrySource == "location")
            {
                string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
                if (Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim() ?? ip;
                }
                
                if (!string.IsNullOrEmpty(ip) && ip != "::1" && ip != "127.0.0.1")
                {
                    try
                    {
                        var client = _httpClientFactory.CreateClient();
                        var ipResponse = await client.GetAsync($"http://ip-api.com/json/{ip}");
                        if (ipResponse.IsSuccessStatusCode)
                        {
                            var ipStr = await ipResponse.Content.ReadAsStringAsync();
                            using var doc = System.Text.Json.JsonDocument.Parse(ipStr);
                            if (doc.RootElement.TryGetProperty("countryCode", out var ccProp))
                            {
                                var c = ccProp.GetString();
                                if (!string.IsNullOrEmpty(c)) country = c;
                            }
                        }
                    }
                    catch { }
                }
            }
            
            if (string.IsNullOrEmpty(country) && userId.HasValue && !string.IsNullOrEmpty(authToken))
            {
                try
                {
                    var client = _httpClientFactory.CreateClient();
                    client.BaseAddress = new Uri("http://ms_users:8080/");
                    client.DefaultRequestHeaders.Add("Authorization", authToken);
                    var profileResponse = await client.GetAsync($"api/Users/profile?userId={userId}&appId={id}");
                    if (profileResponse.IsSuccessStatusCode)
                    {
                        var profileStr = await profileResponse.Content.ReadAsStringAsync();
                        using var doc = System.Text.Json.JsonDocument.Parse(profileStr);
                        if (doc.RootElement.TryGetProperty("dynamicData", out var dynamicData) && 
                            dynamicData.TryGetProperty("countryId", out var countryIdVal))
                        {
                            var c = countryIdVal.GetString();
                            if (!string.IsNullOrEmpty(c)) country = c;
                        }
                    }
                }
                catch { }
            }
        }

        var result = await _mediator.Send(new Apps.Application.Features.Apps.Queries.GetPackagesByApp.GetPackagesByAppQuery(id, country, defaultCountry));
        return Ok(result);
    }

    [HttpGet("{appId}/users/{userId}/subscriptions")]
    public async Task<IActionResult> GetUserSubscriptions(Guid appId, Guid userId)
    {
        var result = await _mediator.Send(new Apps.Application.Features.Subscriptions.Queries.GetUserSubscriptions.GetUserSubscriptionsQuery(appId, userId));
        return Ok(result);
    }

    [HttpPost("{appId}/users/{userId}/subscriptions")]
    public async Task<IActionResult> GrantSubscription(Guid appId, Guid userId, [FromBody] Apps.Application.Features.Subscriptions.Commands.GrantSubscription.GrantSubscriptionCommand command)
    {
        if (appId != command.AppId || userId != command.UserId) return BadRequest();
        var subId = await _mediator.Send(command);
        return Ok(new { SubscriptionId = subId });
    }

    [HttpPut("{appId}/subscriptions/{id}/status")]
    public async Task<IActionResult> ChangeSubscriptionStatus(Guid appId, Guid id, [FromBody] Apps.Application.Features.Subscriptions.Commands.ChangeStatus.ChangeSubscriptionStatusCommand command)
    {
        if (id != command.SubscriptionId) return BadRequest();
        var result = await _mediator.Send(command);
        if (!result) return NotFound();
        return NoContent();
    }
}
