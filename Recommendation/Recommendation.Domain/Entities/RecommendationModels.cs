using Microsoft.ML.Data;
using Shared.Kernel;

namespace Recommendation.Domain.Entities;

public class RecommendationData
{
    [LoadColumn(0)]
    public float UserId { get; set; }

    [LoadColumn(1)]
    public float ItemId { get; set; } // Could be ProfileId, AppId, etc.

    [LoadColumn(2)]
    public float Label { get; set; } // Rating, Like(1)/Dislike(0)
}

public class RecommendationPrediction
{
    public float Score { get; set; }
    public float UserId { get; set; }
    public float ItemId { get; set; }
}

// Interface for the Engine
public interface IRecommendationEngine 
{
    void Train(IEnumerable<RecommendationData> trainingData);
    float Predict(int userId, int itemId);
}
