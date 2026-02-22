using MassTransit;
using Notifications.Application.Common.Interfaces;
using Shared.Messaging.Events;

namespace Notifications.Application.Features.Notifications.EventConsumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ISignalRService _signalRService;

    public UserRegisteredConsumer(IEmailService emailService, ISignalRService signalRService)
    {
        _emailService = emailService;
        _signalRService = signalRService;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        
        string appName = "MS Platform";
        var appIdStr = message.AppId.ToString();
        if (appIdStr == "11111111-1111-1111-1111-111111111111") appName = "FitIT";
        else if (appIdStr == "22222222-2222-2222-2222-222222222222") appName = "Wissler";
        
        string body = GetWelcomeEmail(message.DisplayName, appName, message.InitialPassword);
        
        // Parallel execution
        var emailTask = _emailService.SendEmailAsync(message.Email, $"Welcome to {appName}", body);
        var signalRTask = _signalRService.BroadcastAsync("New User", $"{message.DisplayName} just joined {appName}!");

        await Task.WhenAll(emailTask, signalRTask);
    }

    private string GetWelcomeEmail(string name, string appName, string? initialPassword = null)
    {
        var passwordHtml = !string.IsNullOrWhiteSpace(initialPassword) 
            ? $"<p>Your account was set up by an administrator. Here is your temporary password:</p><p style='font-size: 20px; font-weight: bold; background: #eee; padding: 10px; text-align: center; border-radius: 4px;'>{initialPassword}</p><p>Please reset it upon your first login.</p>"
            : "<p>Your account has been successfully created. You can now log in and explore our services.</p>";

        var body = $@"
            <p>Hi {name},</p>
            <p>Welcome to {appName}! We're excited to have you on board.</p>
            {passwordHtml}
            <a href='#' style='display: inline-block; padding: 12px 24px; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: bold; margin-top: 20px;'>Go to Dashboard</a>";

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
            <h1>Welcome aboard!</h1>
        </div>
        <div class='content'>
            {body}
        </div>
        <p>&copy; {DateTime.Now.Year} {appName}</p>
    </div>
</body>
</html>";
    }
}
