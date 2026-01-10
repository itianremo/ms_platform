namespace Chat.Application.Common.Interfaces;

public interface ITextModerationService
{
    Task<bool> ContainsBannedWordsAsync(string content);
    Task<string> SanitizeContentAsync(string content); // Optional: Mask bad words
}
