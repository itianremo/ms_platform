using Apps.Domain.Repositories;
using MediatR;
using Shared.Kernel;

namespace Apps.Application.Features.Apps.Commands.UpdateApp;

public class UpdateAppCommandHandler : IRequestHandler<UpdateAppCommand, bool>
{
    private readonly IAppRepository _repository;

    public UpdateAppCommandHandler(IAppRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateAppCommand request, CancellationToken cancellationToken)
    {
        var app = await _repository.GetByIdAsync(request.Id);
        if (app == null) return false;

        app.UpdateDetails(request.Name, request.Description, request.BaseUrl);
        await _repository.UpdateAsync(app);
        return true;
    }
}
