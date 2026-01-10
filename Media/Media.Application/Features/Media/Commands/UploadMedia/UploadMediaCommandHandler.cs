using MediatR;
using Media.Application.Common.Interfaces;

namespace Media.Application.Features.Media.Commands.UploadMedia;

public class UploadMediaCommandHandler : IRequestHandler<UploadMediaCommand, string>
{
    private readonly IMediaService _mediaService;
    private readonly IImageModerationService _moderationService;
    private readonly IImageProcessingService _processingService;

    public UploadMediaCommandHandler(IMediaService mediaService, IImageModerationService moderationService, IImageProcessingService processingService)
    {
        _mediaService = mediaService;
        _moderationService = moderationService;
        _processingService = processingService;
    }

    public async Task<string> Handle(UploadMediaCommand request, CancellationToken cancellationToken)
    {
        var metadata = new Dictionary<string, string>();
        
        // Scan for unsafe content
        var (isFlagged, reason) = await _moderationService.ExamineImageAsync(request.FileStream, request.FileName);
        
        if (isFlagged)
        {
            metadata.Add("x-amz-meta-is-flagged", "true");
            metadata.Add("x-amz-meta-moderation-reason", reason ?? "Unknown");
        }

        // Resize if image
        Stream uploadStream = request.FileStream;
        if (request.ContentType.StartsWith("image/"))
        {
             uploadStream = await _processingService.ResizeAsync(request.FileStream, request.ContentType, 1920, 1080);
        }

        return await _mediaService.UploadFileAsync(uploadStream, request.FileName, request.ContentType, metadata);
    }
}
