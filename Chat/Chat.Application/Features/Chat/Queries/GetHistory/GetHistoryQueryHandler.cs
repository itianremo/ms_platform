using Chat.Domain.Entities;
using Chat.Domain.Repositories;
using MediatR;

namespace Chat.Application.Features.Chat.Queries.GetHistory;

public class GetHistoryQueryHandler : IRequestHandler<GetHistoryQuery, List<ChatMessage>>
{
    private readonly IChatRepository _chatRepository;

    public GetHistoryQueryHandler(IChatRepository chatRepository)
    {
        _chatRepository = chatRepository;
    }

    public async Task<List<ChatMessage>> Handle(GetHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _chatRepository.GetHistoryAsync(request.SenderId, request.RecipientId);
    }
}
