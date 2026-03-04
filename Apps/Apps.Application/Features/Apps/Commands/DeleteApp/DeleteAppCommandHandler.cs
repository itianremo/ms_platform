using MediatR;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Commands.DeleteApp;

public class DeleteAppCommandHandler : IRequestHandler<DeleteAppCommand, bool>
{
    private readonly IAppRepository _appRepository;

    public DeleteAppCommandHandler(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }

    public async Task<bool> Handle(DeleteAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _appRepository.GetByIdAsync(request.Id);
        if (app == null) return false;

        await _appRepository.DeleteAsync(app);
        return true;
    }
}
