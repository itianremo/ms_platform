using Media.Application.Features.Media.Commands.UploadMedia;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;

    public MediaController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is empty.");

        using var stream = file.OpenReadStream();
        var command = new UploadMediaCommand(stream, file.FileName, file.ContentType);
        
        var result = await _mediator.Send(command);
        
        return Ok(new { Url = result });
    }
    [HttpGet("flagged")]
    public IActionResult GetFlagged()
    {
        // Mock Data for Frontend Development
        var mockData = new[]
        {
            new { Id = Guid.NewGuid(), FileName = "unsafe_image.jpg", Url = "http://localhost:9000/media/unsafe_image.jpg", Reason = "NSFW detected" },
            new { Id = Guid.NewGuid(), FileName = "spam_banner.png", Url = "http://localhost:9000/media/spam_banner.png", Reason = "Text overlay spam" }
        };
        return Ok(mockData);
    }
}
