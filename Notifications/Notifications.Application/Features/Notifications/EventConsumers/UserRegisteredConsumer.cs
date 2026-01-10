using MassTransit;
using Notifications.Application.Common.Interfaces;
using Shared.Messaging.Events;

namespace Notifications.Application.Features.Notifications.EventConsumers;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;

    public UserRegisteredConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;
        await _emailService.SendEmailAsync(message.Email, "Welcome!", $"Hello {message.DisplayName}, welcome to the platform!");
    }
}
