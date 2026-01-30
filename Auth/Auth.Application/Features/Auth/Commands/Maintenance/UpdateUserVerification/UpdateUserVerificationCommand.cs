using MediatR;
using Auth.Domain.Repositories;
using Shared.Kernel;

namespace Auth.Application.Features.Auth.Commands.Maintenance.UpdateUserVerification;

public record UpdateUserVerificationCommand(Guid UserId, string Type, bool Verified) : IRequest;

public class UpdateUserVerificationCommandHandler : IRequestHandler<UpdateUserVerificationCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserVerificationCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(UpdateUserVerificationCommand request, CancellationToken cancellationToken)
    {
         var user = await _userRepository.GetByIdAsync(request.UserId);
         if (user == null) throw new Shared.Kernel.Exceptions.NotFoundException(nameof(global::Auth.Domain.Entities.User), request.UserId);

         if (request.Type.Equals("email", StringComparison.OrdinalIgnoreCase))
         {
             user.SetEmailVerified(request.Verified);
         }
         else if (request.Type.Equals("phone", StringComparison.OrdinalIgnoreCase))
         {
             user.SetPhoneVerified(request.Verified);
         }
         
         await _userRepository.UpdateAsync(user);
    }
}
