using Microsoft.Extensions.Logging;
using Notifications.Application.Common.Interfaces;

namespace Notifications.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation($"Sending Email to {to}: Subject: {subject}, Body: {body}");
        return Task.CompletedTask;
    }
}
