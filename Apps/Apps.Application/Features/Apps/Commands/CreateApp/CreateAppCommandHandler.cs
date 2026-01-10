using MediatR;
using Apps.Domain.Entities;
using Apps.Domain.Repositories;

namespace Apps.Application.Features.Apps.Commands.CreateApp;

public class CreateAppCommandHandler : IRequestHandler<CreateAppCommand, Guid>
{
    private readonly IAppRepository _appRepository;

    public CreateAppCommandHandler(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }

    public async Task<Guid> Handle(CreateAppCommand request, CancellationToken cancellationToken)
    {
        var existingApp = await _appRepository.GetByNameAsync(request.Name);
        if (existingApp != null)
        {
            throw new InvalidOperationException("App with this name already exists.");
        }

        var app = new AppConfig(request.Name, request.Description, request.BaseUrl);
        await _appRepository.AddAsync(app);

        return app.Id;
    }
}
