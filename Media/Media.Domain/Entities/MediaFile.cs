using Shared.Kernel;

namespace Media.Domain.Entities;

public class MediaFile : Entity
{
    public string FileName { get; private set; }
    public string Url { get; private set; }
    public long SizeBytes { get; private set; }
    public string ContentType { get; private set; }
    public bool IsFlagged { get; private set; }
    public string? ModerationReason { get; private set; }

    private MediaFile() { }

    public MediaFile(string fileName, string url, long sizeBytes, string contentType)
    {
        Id = Guid.NewGuid();
        FileName = fileName;
        Url = url;
        SizeBytes = sizeBytes;
        ContentType = contentType;
        IsFlagged = false;
    }

    public void Flag(string reason)
    {
        IsFlagged = true;
        ModerationReason = reason;
    }
}
