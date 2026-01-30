using Apps.Domain.Repositories;
using MediatR;

namespace Apps.Application.Features.Apps.Commands.UpdateExternalAuthConfig;

public class UpdateExternalAuthConfigCommandHandler : IRequestHandler<UpdateExternalAuthConfigCommand, bool>
{
    private readonly IAppRepository _appRepository;

    public UpdateExternalAuthConfigCommandHandler(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }

    public async Task<bool> Handle(UpdateExternalAuthConfigCommand request, CancellationToken cancellationToken)
    {
        var app = await _appRepository.GetByIdAsync(request.Id);
        if (app == null) return false;

        app.UpdateExternalAuthProviders(request.ExternalLoginsJson);
        await _appRepository.UpdateAsync(app);
        
        return true;
    }
}
