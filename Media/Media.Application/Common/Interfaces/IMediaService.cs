namespace Media.Application.Common.Interfaces;

public interface IMediaService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, Dictionary<string, string>? metadata = null);
    Task<Stream> GetFileAsync(string fileName);
    Task<string> GetPresignedUrlAsync(string fileName, int expirySeconds = 3600);
}
