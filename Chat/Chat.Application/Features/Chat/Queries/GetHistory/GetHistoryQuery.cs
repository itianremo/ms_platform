using Chat.Domain.Entities;
using MediatR;

namespace Chat.Application.Features.Chat.Queries.GetHistory;

public record GetHistoryQuery(Guid SenderId, Guid RecipientId) : IRequest<List<ChatMessage>>;
