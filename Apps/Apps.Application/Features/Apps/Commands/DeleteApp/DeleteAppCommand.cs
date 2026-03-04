using MediatR;

namespace Apps.Application.Features.Apps.Commands.DeleteApp;

public class DeleteAppCommand : IRequest<bool>
{
    public Guid Id { get; set; }

    public DeleteAppCommand(Guid id)
    {
        Id = id;
    }
}
