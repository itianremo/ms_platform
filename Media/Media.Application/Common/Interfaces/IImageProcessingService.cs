namespace Media.Application.Common.Interfaces;

public interface IImageProcessingService
{
    Task<Stream> ResizeAsync(Stream input, string contentType, int maxWidth, int maxHeight);
}
