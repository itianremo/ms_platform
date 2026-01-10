using Microsoft.ML;
using Microsoft.ML.Trainers;
using Recommendation.Domain.Entities;

namespace Recommendation.Infrastructure.Services;

public class RecommendationEngine : IRecommendationEngine
{
    private readonly MLContext _mlContext;
    private ITransformer _model;

    public RecommendationEngine()
    {
        _mlContext = new MLContext();
    }

    public void Train(IEnumerable<RecommendationData> trainingData)
    {
        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

        // Matrix Factorization
        var options = new MatrixFactorizationTrainer.Options
        {
            MatrixColumnIndexColumnName = nameof(RecommendationData.UserId),
            MatrixRowIndexColumnName = nameof(RecommendationData.ItemId),
            LabelColumnName = nameof(RecommendationData.Label),
            NumberOfIterations = 20,
            ApproximationRank = 100
        };

        var pipeline = _mlContext.Transforms.Conversion.MapValueToKey(nameof(RecommendationData.UserId))
            .Append(_mlContext.Transforms.Conversion.MapValueToKey(nameof(RecommendationData.ItemId)))
            .Append(_mlContext.Recommendation().Trainers.MatrixFactorization(options));

        _model = pipeline.Fit(dataView);
    }

    public float Predict(int userId, int itemId)
    {
        if (_model == null) return 0; // Or throw

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<RecommendationData, RecommendationPrediction>(_model);
        
        var prediction = predictionEngine.Predict(new RecommendationData
        {
            UserId = userId,
            ItemId = itemId
        });

        return prediction.Score;
    }
}
