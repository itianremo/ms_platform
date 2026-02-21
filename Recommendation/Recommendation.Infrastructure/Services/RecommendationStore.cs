using System.Collections.Concurrent;
using Recommendation.Application.Common.Interfaces;

namespace Recommendation.Infrastructure.Services;

public class InMemoryRecommendationStore : IRecommendationStore
{
    // Key: UserId, Value: List of (TargetId, Action)
    private readonly ConcurrentDictionary<Guid, List<(Guid TargetId, string Action)>> _swipes = new();
    
    // Key: UserId, Value: (DailyCount, FirstLikeTime)
    private readonly ConcurrentDictionary<Guid, (int Count, DateTime? FirstLikeTime)> _param = new();

    private const int MAX_LIKES = 25;

    public void AddSwipe(Guid userId, Guid targetId, string action)
    {
        _swipes.AddOrUpdate(userId, 
            new List<(Guid, string)> { (targetId, action) },
            (key, list) => { list.Add((targetId, action)); return list; });
            
        if (action == "Like")
        {
            _param.AddOrUpdate(userId,
                (1, DateTime.UtcNow),
                (key, val) => 
                {
                    // Check if 24h passed
                    if (val.FirstLikeTime.HasValue && (DateTime.UtcNow - val.FirstLikeTime.Value).TotalHours >= 24)
                    {
                        return (1, DateTime.UtcNow); // Reset
                    }
                    return (val.Count + 1, val.FirstLikeTime ?? DateTime.UtcNow);
                });
        }
    }

    public List<Guid> GetSwipedUsers(Guid userId)
    {
        if (_swipes.TryGetValue(userId, out var list))
        {
            return list.Select(x => x.TargetId).ToList();
        }
        return new List<Guid>();
    }

    public bool IsMatch(Guid userId, Guid targetId)
    {
        // Check if User Liked Target AND Target Liked User
        if (_swipes.TryGetValue(userId, out var userSwipes) && 
            _swipes.TryGetValue(targetId, out var targetSwipes))
        {
            var userLiked = userSwipes.Any(x => x.TargetId == targetId && x.Action == "Like");
            var targetLiked = targetSwipes.Any(x => x.TargetId == userId && x.Action == "Like");
            return userLiked && targetLiked;
        }
        return false;
    }

    public (int Count, DateTime? FirstLikeTime, int Remaining) GetSwipeInfo(Guid userId)
    {
        if (_param.TryGetValue(userId, out var val))
        {
             // Check if 24h passed (Read-only check)
            if (val.FirstLikeTime.HasValue && (DateTime.UtcNow - val.FirstLikeTime.Value).TotalHours >= 24)
            {
                return (0, null, MAX_LIKES);
            }
            int remaining = MAX_LIKES - val.Count;
            return (val.Count, val.FirstLikeTime, remaining < 0 ? 0 : remaining);
        }
        return (0, null, MAX_LIKES);
    }
}
