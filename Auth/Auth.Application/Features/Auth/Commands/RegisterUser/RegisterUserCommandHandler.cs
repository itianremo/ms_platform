using MediatR;
using Auth.Domain.Entities;
using Auth.Domain.Repositories;
using Auth.Application.Common.Interfaces;
using MassTransit;
using Shared.Messaging.Events;

namespace Auth.Application.Features.Auth.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IPublishEndpoint _publishEndpoint;

    public RegisterUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IPublishEndpoint publishEndpoint)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);
        
        var user = new User(request.Email, request.Phone, passwordHash);

        await _userRepository.AddAsync(user);

        // Publish Event
        await _publishEndpoint.Publish(new UserRegisteredEvent(
            user.Id, 
            request.AppId ?? Guid.Empty, 
            user.Email, 
            user.Email // DisplayName defaults to Email initially
        ), cancellationToken);

        return user.Id;
    }
}
