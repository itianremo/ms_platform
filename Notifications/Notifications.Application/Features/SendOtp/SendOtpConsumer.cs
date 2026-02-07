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
                string emailBody = GetOtpEmail(message.Code);
                await _emailService.SendEmailAsync(message.Destination, "Verification Code", emailBody);
            }
            else if (message.Type == "Reactivation")
            {
                // Reactivation Link
                string link = $"http://localhost:5173/reactivate?email={System.Net.WebUtility.UrlEncode(message.Destination)}&token={message.Code}";
                string emailBody = $"<h3>Account Reactivation</h3><p>Your account was previously deleted.</p><p>Click the link below to restore your account:</p><p><a href=\"{link}\">Reactivate Account</a></p>";
                await _emailService.SendEmailAsync(message.Destination, "Reactivate Account", emailBody);
            }
            else if (message.Type == "PasswordReset")
            {
                string emailBody = GetOtpEmail(message.Code).Replace("Verification Code", "Reset Password Code").Replace("verification code for your account", "password reset code");
                await _emailService.SendEmailAsync(message.Destination, "Reset Password Request", emailBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP.");
            throw; 
        }
    }

    private string GetOtpEmail(string otpCode)
    {
        var body = $@"
            <p>Hello,</p>
            <p>You requested a verification code for your account. Please use the following code to complete your request:</p>
            <div style='font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #2563eb; margin: 20px 0; text-align: center; background: #f0f7ff; padding: 15px; border-radius: 4px;'>{otpCode}</div>
            <p>This code will expire in 10 minutes.</p>
            <p>If you did not request this code, please ignore this email.</p>";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }}
        .header {{ background-color: #2563eb; color: white; padding: 10px; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Verification Code</h1>
        </div>
        <div class='content'>
            {body}
        </div>
        <p>&copy; {DateTime.Now.Year} MS Platform</p>
    </div>
</body>
</html>";
    }
}
