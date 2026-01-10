using MediatR;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Repositories;

namespace Auth.Application.Features.Auth.Commands.DeleteUser;

public record DeleteUserCommand(string Email) : IRequest;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null) return;

        if (user.IsSealed)
        {
            throw new InvalidOperationException("Cannot delete a sealed user.");
        }

        user.SoftDelete();
        await _userRepository.UpdateAsync(user);
    }
}
