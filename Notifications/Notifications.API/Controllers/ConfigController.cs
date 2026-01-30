using Microsoft.AspNetCore.Mvc;
using Notifications.Application.Common.Interfaces;
using Notifications.Domain.Entities;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly INotificationConfigRepository _repository;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;

    public ConfigController(INotificationConfigRepository repository, ISmsService smsService, IEmailService emailService)
    {
        _repository = repository;
        _smsService = smsService;
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var configs = await _repository.GetAllAsync(cancellationToken);
        return Ok(configs);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] NotificationConfig config, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByTypeAsync(config.Type, cancellationToken);
        if (existing != null)
        {
            existing.Provider = config.Provider;
            existing.ConfigJson = config.ConfigJson;
            existing.IsActive = config.IsActive;
            await _repository.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await _repository.AddAsync(config, cancellationToken);
        }
        return Ok();
    }

    [HttpPost("test-sms")]
    public async Task<IActionResult> TestSms([FromBody] TestSmsRequest request)
    {
        try
        {
            // This relies on the internal SmsService implementation which presumably reads "Active" config from Repo on its own 
            // OR checks generic provider.
            // Our previous SmsService modification added checks for active config.
            await _smsService.SendSmsAsync(request.Phone, request.Message);
            return Ok(new { success = true, message = "SMS sent (or queued)" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
    {
        try
        {
            await _emailService.SendEmailAsync(request.Email, "Test Email", "This is a test email from your Global Admin Dashboard configuration.");
            return Ok(new { success = true, message = "Email sent (or queued)" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public record TestSmsRequest(string Phone, string Message);
public record TestEmailRequest(string Email);
