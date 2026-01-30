using Apps.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace Apps.Application.Features.Apps.Commands.DeactivateApp;

public class DeactivateAppCommandHandler : IRequestHandler<DeactivateAppCommand, bool>
{
    private readonly IAppRepository _repository;

    public DeactivateAppCommandHandler(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeactivateAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.Id);
        if (app == null) return false;

        if (request.IsActive)
            app.Activate();
        else
            app.Deactivate();

        await _repository.UpdateAsync(app);
        return true;
    }
}
