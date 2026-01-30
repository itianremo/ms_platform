using Microsoft.Extensions.Logging;
using Notifications.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Notifications.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IServiceProvider serviceProvider, ILogger<EmailService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<INotificationConfigRepository>();
            var configEntity = await repository.GetByTypeAsync("Email", CancellationToken.None) 
                             ?? await repository.GetByTypeAsync("Smtp", CancellationToken.None);

            if (configEntity == null || string.IsNullOrEmpty(configEntity.ConfigJson))
            {
                _logger.LogWarning("SMTP Configuration not found or empty. Email not sent.");
                return;
            }

            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var config = System.Text.Json.JsonSerializer.Deserialize<SmtpConfigModel>(configEntity.ConfigJson, options);
            if (config == null) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(config.FromName, config.FromAddress));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    // Gmail requires StartTls on 587
                    await client.ConnectAsync(config.Host, int.Parse(config.Port), config.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);
                    
                    // Note: If user set Port 465, SecureSocketOptions.SslOnConnect might be better, 
                    // but StartTls is standard for 587. Auto covers most cases.

                    if (!string.IsNullOrEmpty(config.Username) && !string.IsNullOrEmpty(config.Password))
                    {
                        await client.AuthenticateAsync(config.Username, config.Password);
                    }

                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                    _logger.LogInformation($"Email sent successfully to {to}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email to {to}");
                    throw; // Rethrow to let caller know
                }
            }
        }
    }

    private class SmtpConfigModel
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public bool EnableSsl { get; set; }
    }
}
