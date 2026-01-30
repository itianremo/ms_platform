using Microsoft.AspNetCore.Mvc;
using Notifications.Domain.Entities;
using Notifications.Infrastructure.Repositories;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IUserNotificationRepository _repository;

    public NotificationsController(IUserNotificationRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        // TODO: Get UserId from Claims. For now, we might rely on a query param or header for testing if auth middleware isn't fully setting User.Identity
        // But usually: var userId = Guid.Parse(User.FindFirst("sub")?.Value);
        
        // Mocking user ID for now as we might not have the full auth context populated in this dev environment or it varies
        // Assuming a standard header or just a test ID if missing.
        // Let's try to grab from query for flexibility in this "Admin" context if strict auth isn't enforced yet.
        
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
        {
             // Fallback or Unauthorized. For development speed, let's check Query as well
             if (!Guid.TryParse(Request.Query["userId"], out userId))
             {
                 return BadRequest("User ID required");
             }
        }

        var notifications = await _repository.GetAllByUserIdAsync(userId, 50, cancellationToken);
        return Ok(notifications);
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread(CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
        {
             if (!Guid.TryParse(Request.Query["userId"], out userId)) return BadRequest("User ID required");
        }

        var notifications = await _repository.GetUnreadByUserIdAsync(userId, cancellationToken);
        return Ok(notifications);
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _repository.MarkAsReadAsync(id, cancellationToken);
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) || !Guid.TryParse(userIdHeader, out var userId))
        {
             if (!Guid.TryParse(Request.Query["userId"], out userId)) return BadRequest("User ID required");
        }

        await _repository.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok();
    }

    // Endpoint to create a notification (internal use or for testing)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        var notif = new UserNotification(request.UserId, request.Title, request.Message, request.Link);
        await _repository.AddAsync(notif, cancellationToken);
        return Ok();
    }
}

public record CreateNotificationRequest(Guid UserId, string Title, string Message, string? Link);
