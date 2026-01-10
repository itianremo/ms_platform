using System.Text.RegularExpressions;
using Chat.Application.Common.Interfaces;

namespace Chat.Infrastructure.Services;

public class TextModerationService : ITextModerationService
{
    // Simple mock list of banned words
    private static readonly List<string> _bannedWords = new() { "badword", "hate", "spam" };

    public Task<bool> ContainsBannedWordsAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Task.FromResult(false);

        var normalized = content.ToLowerInvariant();
        bool hasBadWord = _bannedWords.Any(word => normalized.Contains(word));
        
        return Task.FromResult(hasBadWord);
    }

    public Task<string> SanitizeContentAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return Task.FromResult(content);

        var sanitized = content;
        foreach (var word in _bannedWords)
        {
            sanitized = Regex.Replace(sanitized, word, "***", RegexOptions.IgnoreCase);
        }

        return Task.FromResult(sanitized);
    }
}
