using Media.Application.Common.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Media.Infrastructure.Services;

public class ImageProcessingService : IImageProcessingService
{
    public async Task<Stream> ResizeAsync(Stream input, string contentType, int maxWidth, int maxHeight)
    {
        // Reset stream if needed
        if (input.CanSeek) input.Position = 0;

        using var image = await Image.LoadAsync(input);

        // Only resize if larger than target
        if (image.Width > maxWidth || image.Height > maxHeight)
        {
            var options = new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max
            };

            image.Mutate(x => x.Resize(options));
        }

        var outStream = new MemoryStream();
        
        // Save back to stream in original format
        if (contentType.Contains("png"))
            await image.SaveAsPngAsync(outStream);
        else if (contentType.Contains("gif"))
            await image.SaveAsGifAsync(outStream);
        else if (contentType.Contains("bmp"))
            await image.SaveAsBmpAsync(outStream);
        else 
            await image.SaveAsJpegAsync(outStream); // Default to JPEG

        outStream.Position = 0;
        return outStream;
    }
}
