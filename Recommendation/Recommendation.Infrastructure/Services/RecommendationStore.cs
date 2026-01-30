using System.Collections.Concurrent;

namespace Recommendation.Infrastructure.Services;

public interface IRecommendationStore
{
    void AddSwipe(int userId, int targetId, string action);
    List<int> GetSwipedUsers(int userId);
    bool IsMatch(int userId, int targetId);
}

public class InMemoryRecommendationStore : IRecommendationStore
{
    // Key: UserId, Value: List of (TargetId, Action)
    private readonly ConcurrentDictionary<int, List<(int TargetId, string Action)>> _swipes = new();

    public void AddSwipe(int userId, int targetId, string action)
    {
        _swipes.AddOrUpdate(userId, 
            new List<(int, string)> { (targetId, action) },
            (key, list) => { list.Add((targetId, action)); return list; });
    }

    public List<int> GetSwipedUsers(int userId)
    {
        if (_swipes.TryGetValue(userId, out var list))
        {
            return list.Select(x => x.TargetId).ToList();
        }
        return new List<int>();
    }

    public bool IsMatch(int userId, int targetId)
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
}
