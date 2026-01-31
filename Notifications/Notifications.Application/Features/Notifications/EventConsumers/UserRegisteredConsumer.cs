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
        string body = GetWelcomeEmail(message.DisplayName);
        
        // Parallel execution
        var emailTask = _emailService.SendEmailAsync(message.Email, "Welcome to MS Platform", body);
        var signalRTask = _signalRService.BroadcastAsync("New User", $"{message.DisplayName} just joined!");

        await Task.WhenAll(emailTask, signalRTask);
    }

    private string GetWelcomeEmail(string name)
    {
        var body = $@"
            <p>Hi {name},</p>
            <p>Welcome to MS Platform! We're excited to have you on board.</p>
            <p>Your account has been successfully created. You can now log in and explore our services.</p>
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
        <p>&copy; {DateTime.Now.Year} MS Platform</p>
    </div>
</body>
</html>";
    }
}
