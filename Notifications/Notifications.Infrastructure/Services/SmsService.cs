using Microsoft.Extensions.Logging;
using Notifications.Application.Common.Interfaces;

namespace Notifications.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    private readonly INotificationConfigRepository _repository;

    public SmsService(ILogger<SmsService> logger, INotificationConfigRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task SendSmsAsync(string phoneNumber, string message)
    {
        // Check Config
        var config = await _repository.GetByTypeAsync("Sms", CancellationToken.None);
        if (config == null || !config.IsActive)
        {
             _logger.LogWarning($"[Mock Twilio] SMS Configuration missing or inactive. Failed to send to {phoneNumber}.");
             return; // Silent fail in background or throw? Throwing might retry. 
             // Requirement: "if no otp... give toast". Frontend check handles toast. Backend should just not send.
        }

        // MOCK TWILIO IMPLEMENTATION
        // For now, we just log it. The static OTP "1234" is generated in the Auth module, 
        // but here we simulate the "Sending" via a provider.
        
        _logger.LogInformation($"[Mock Twilio] Sending SMS to {phoneNumber}: {message}");
        
        // Simulate network delay
        await Task.Delay(50);
        
        _logger.LogInformation($"[Mock Twilio] SMS Sent Successfully to {phoneNumber}");
    }
}
