using MassTransit;
using Shared.Messaging.Events;
using Notifications.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Notifications.Application.Features.SendOtp;

public class SendOtpConsumer : IConsumer<SendOtpEvent>
{
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SendOtpConsumer> _logger;

    public SendOtpConsumer(ISmsService smsService, IEmailService emailService, ILogger<SendOtpConsumer> logger)
    {
        _smsService = smsService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendOtpEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation($"Processing SendOtpEvent for {message.Destination} (Type: {message.Type})");

        try 
        {
            if (message.Type == "Phone")
            {
                // Send SMS
                string smsBody = $"Your Verification Code is: {message.Code}. It expires in 5 minutes.";
                await _smsService.SendSmsAsync(message.Destination, smsBody);
            }
            else if (message.Type == "Email")
            {
                // Send Email
                string emailBody = $"Your Verification Code is: {message.Code}. It expires in 5 minutes.";
                await _emailService.SendEmailAsync(message.Destination, "Verification Code", emailBody);
            }
            else if (message.Type == "Reactivation")
            {
                // Reactivation Link
                string link = $"http://localhost:5173/reactivate?email={System.Net.WebUtility.UrlEncode(message.Destination)}&token={message.Code}";
                string emailBody = $"<h3>Account Reactivation</h3><p>Your account was previously deleted.</p><p>Click the link below to restore your account:</p><p><a href=\"{link}\">Reactivate Account</a></p>";
                await _emailService.SendEmailAsync(message.Destination, "Reactivate Account", emailBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP.");
            // Ideally publish a Failed event or retry?
            throw; // Let MassTransit retry
        }
    }
}
