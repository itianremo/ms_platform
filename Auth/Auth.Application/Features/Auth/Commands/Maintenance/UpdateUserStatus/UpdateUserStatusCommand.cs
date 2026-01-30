using MediatR;
using Auth.Domain.Repositories;
using Auth.Domain.Entities;
using Shared.Kernel; // For potential Exceptions

namespace Auth.Application.Features.Auth.Commands.Maintenance.UpdateUserStatus;

public record UpdateUserStatusCommand(Guid UserId, GlobalUserStatus NewStatus) : IRequest;

public class UpdateUserStatusCommandHandler : IRequestHandler<UpdateUserStatusCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;
    // private readonly ICurrentUserService _currentUserService; // Assuming existence, or pass via command

    public UpdateUserStatusCommandHandler(IUserRepository userRepository, MassTransit.IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(UpdateUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserWithRolesAsync(request.UserId);
        
        if (user == null) 
        {
             user = await _userRepository.GetByIdAsync(request.UserId);
        }

        if (user == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(User), request.UserId);

        var oldStatus = user.Status;
        if (oldStatus == request.NewStatus) return;

        user.SetStatus(request.NewStatus);
        
        await _userRepository.UpdateAsync(user);

        // Publish Event
        await _publishEndpoint.Publish(new Shared.Messaging.Events.UserStatusChangedEvent(
            user.Id,
            (Shared.Kernel.Enums.GlobalUserStatus)oldStatus,
            (Shared.Kernel.Enums.GlobalUserStatus)request.NewStatus,
            null, // PerformedBy - would need ICurrentUserService
            DateTime.UtcNow
        ), cancellationToken);
    }
}
