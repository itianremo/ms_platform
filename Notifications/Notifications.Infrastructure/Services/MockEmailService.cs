using Microsoft.Extensions.Logging;
using Notifications.Application.Common.Interfaces;

namespace Notifications.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;
    private readonly string _logPath;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
        _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_logs");
        if (!Directory.Exists(_logPath)) Directory.CreateDirectory(_logPath);
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Sending Email to {To}: {Subject}", to, subject);

        // Log to file for verification
        string fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid()}.txt";
        string filePath = Path.Combine(_logPath, fileName);
        
        string content = $"To: {to}\nSubject: {subject}\nDate: {DateTime.UtcNow}\n\n{body}";
        
        await File.WriteAllTextAsync(filePath, content);
    }
}
