namespace Media.Application.Common.Interfaces;

public interface IImageModerationService
{
    Task<(bool IsFlagged, string? Reason)> ExamineImageAsync(Stream imageStream, string fileName);
}
