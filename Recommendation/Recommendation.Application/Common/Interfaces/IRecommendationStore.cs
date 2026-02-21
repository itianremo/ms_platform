namespace Recommendation.Application.Common.Interfaces;

public interface IRecommendationStore
{
    void AddSwipe(Guid userId, Guid targetId, string action);
    List<Guid> GetSwipedUsers(Guid userId);
    bool IsMatch(Guid userId, Guid targetId);
    (int Count, DateTime? FirstLikeTime, int Remaining) GetSwipeInfo(Guid userId);
}
