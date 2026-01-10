using Media.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Media.Infrastructure.Services;

public class MinioService : IMediaService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioService> _logger;

    public MinioService(IMinioClient minioClient, IOptions<MinioSettings> settings, ILogger<MinioService> logger)
    {
        _minioClient = minioClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, Dictionary<string, string>? metadata = null)
    {
        try 
        {
            var beArgs = new BucketExistsArgs().WithBucket(_settings.BucketName);
            bool found = await _minioClient.BucketExistsAsync(beArgs);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs().WithBucket(_settings.BucketName);
                await _minioClient.MakeBucketAsync(mbArgs);
            }

            // Ensure unique filename to prevent overwrites
            var objectName = $"{Guid.NewGuid()}_{fileName}";

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType);

            if (metadata != null)
            {
                putObjectArgs.WithHeaders(metadata);
            }

            await _minioClient.PutObjectAsync(putObjectArgs);

            // Construct accessible URL (assuming public bucket or proxied access)
            // For now, returning the object name or internal path
            return objectName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to MinIO");
            throw;
        }
    }

    public async Task<Stream> GetFileAsync(string fileName)
    {
        var memoryStream = new MemoryStream();
        var args = new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(fileName)
            .WithCallbackStream((stream) =>
            {
                stream.CopyTo(memoryStream);
            });

        await _minioClient.GetObjectAsync(args);
        memoryStream.Position = 0;
        return memoryStream;
    }
}
