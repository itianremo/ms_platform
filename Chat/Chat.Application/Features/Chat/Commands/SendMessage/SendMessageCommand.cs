using MediatR;

namespace Chat.Application.Features.Chat.Commands.SendMessage;

public record SendMessageCommand(Guid AppId, Guid SenderId, Guid RecipientId, string Content) : IRequest<Guid>;
