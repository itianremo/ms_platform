using Microsoft.Extensions.Logging;
using Notifications.Application.Common.Interfaces;

namespace Notifications.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message)
    {
        _logger.LogInformation($"Sending SMS to {phoneNumber}: {message}");
        return Task.CompletedTask;
    }
}
