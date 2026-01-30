using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Auth.Application.Features.Auth.Commands.UpdateUserContact;

public class UpdateUserContactCommandHandler : IRequestHandler<UpdateUserContactCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UpdateUserContactCommandHandler> _logger;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;

    public UpdateUserContactCommandHandler(IUserRepository userRepository, ILogger<UpdateUserContactCommandHandler> logger, MassTransit.IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<bool> Handle(UpdateUserContactCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found via ID.");
        }

        bool changed = false;

        // Update Email
        if (!string.IsNullOrEmpty(request.NewEmail) && request.NewEmail != user.Email)
        {
             // Check if email taken
             var existing = await _userRepository.GetByEmailAsync(request.NewEmail);
             if (existing != null && existing.Id != user.Id)
             {
                 throw new InvalidOperationException("Email is already in use.");
             }

             user.UpdateEmail(request.NewEmail);
             user.SetEmailVerified(false); // Invalidate
             changed = true;
        }

        // Update Phone
        if (request.NewPhone != null && request.NewPhone != user.Phone)
        {
             // Check if phone taken
             var existing = await _userRepository.GetByEmailOrPhoneAsync(request.NewPhone);
             if (existing != null && existing.Id != user.Id)
             {
                 throw new InvalidOperationException("Phone number is already in use.");
             }

             user.UpdatePhone(request.NewPhone);
             user.SetPhoneVerified(false); // Invalidate
             changed = true;
        }

        if (changed)
        {
            // Reset Status to trigger re-verification logic on next login
            user.SetStatus(GlobalUserStatus.PendingAccountVerification);
            
            await _userRepository.UpdateAsync(user);
            
            // Sync with Users Service
            await _publishEndpoint.Publish(new Shared.Messaging.Events.UserContactUpdatedEvent(user.Id, user.Email, user.Phone), cancellationToken);
            
            _logger.LogInformation("User {UserId} updated contact info. Verification reset and sync event published.", user.Id);
            return true;
        }

        return false;
    }
}
