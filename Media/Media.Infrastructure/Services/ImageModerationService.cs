using Media.Application.Common.Interfaces;

namespace Media.Infrastructure.Services;

public class ImageModerationService : IImageModerationService
{
    public Task<(bool IsFlagged, string? Reason)> ExamineImageAsync(Stream imageStream, string fileName)
    {
        // Mock Logic: Flag if filename contains "unsafe"
        if (fileName.ToLowerInvariant().Contains("unsafe"))
        {
            return Task.FromResult((true, "Filename indicates unsafe content."));
        }
        
        // In real world: Call AWS Rekognition / Azure Vision here using imageStream
        
        return Task.FromResult((false, (string?)null));
    }
}
