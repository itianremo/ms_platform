using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MediatR;

namespace Chat.Application.Features.Chat.Commands.SendMessage;

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Guid>
{
    private readonly IChatRepository _chatRepository;

    public SendMessageCommandHandler(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<Guid> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var message = new ChatMessage(request.AppId, request.SenderId, request.RecipientId, request.Content);
        await _chatRepository.AddMessageAsync(message);
        return message.Id;
    }
}
