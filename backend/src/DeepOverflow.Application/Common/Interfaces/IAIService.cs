namespace DeepOverflow.Application.Common.Interfaces;

/// <summary>
/// AI service for intelligent features
/// </summary>
public interface IAIService
{
    Task<string> SummarizeAnswerAsync(string answerText, CancellationToken cancellationToken = default);
    Task<List<SimilarQuestionAI>> DetectSimilarQuestionsAsync(string title, string body, CancellationToken cancellationToken = default);
    Task<List<ExpertRecommendation>> RecommendExpertsAsync(Guid questionId, List<string> tags, CancellationToken cancellationToken = default);
    Task<bool> IsSpamAsync(string content, CancellationToken cancellationToken = default);
    Task<List<string>> SuggestTagsAsync(string title, string body, CancellationToken cancellationToken = default);
}

public class SimilarQuestionAI
{
    public Guid QuestionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public float ConfidenceScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class ExpertRecommendation
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Reputation { get; set; }
    public float MatchScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}
