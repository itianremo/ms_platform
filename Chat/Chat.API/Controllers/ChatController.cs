using Chat.Application.Features.Chat.Commands.SendMessage;
using Chat.Application.Features.Chat.Queries.GetHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Chat.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly Chat.Domain.Repositories.IChatRepository _repository;

    public ChatController(IMediator mediator, Chat.Domain.Repositories.IChatRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageCommand command)
    {
        var messageId = await _mediator.Send(command);
        return CreatedAtAction(nameof(SendMessage), new { id = messageId }, new { Id = messageId });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory([FromQuery] Guid senderId, [FromQuery] Guid recipientId)
    {
        var result = await _mediator.Send(new GetHistoryQuery(senderId, recipientId));
        return Ok(result);
    }

    [HttpGet("flagged")]
    public async Task<IActionResult> GetFlaggedMessages([FromQuery] Guid appId)
    {
        if (appId == Guid.Empty)
           return BadRequest("AppId is required.");

        var messages = await _repository.GetFlaggedAsync(appId);
        return Ok(messages);
    }
}
