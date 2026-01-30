using Media.Application.Common.Interfaces;
using Media.Application.Features.Media.Commands.UploadMedia;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMediaService _mediaService;

    public MediaController(IMediator mediator, IMediaService mediaService)
    {
        _mediator = mediator;
        _mediaService = mediaService;
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
    [HttpGet("{id}/url")]
    public async Task<IActionResult> GetUrl(string id)
    {
        // 'id' is currently the object name or filename
        try 
        {
            var url = await _mediaService.GetPresignedUrlAsync(id);
            return Ok(new { Url = url });
        }
        catch (Exception ex)
        {
            return NotFound(ex.Message);
        }
    }
}
