using DeepOverflow.Application.Common.Interfaces;

namespace DeepOverflow.Infrastructure.Services;

public sealed class NoOpAIService : IAIService
{
    public Task<string> SummarizeAnswerAsync(string answerText, CancellationToken cancellationToken = default)
    {
        var text = (answerText ?? string.Empty).Trim();
        return Task.FromResult(text.Length <= 500 ? text : text[..500]);
    }

    public Task<List<SimilarQuestionAI>> DetectSimilarQuestionsAsync(string title, string body, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<SimilarQuestionAI>());

    public Task<List<ExpertRecommendation>> RecommendExpertsAsync(Guid questionId, List<string> tags, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<ExpertRecommendation>());

    public Task<bool> IsSpamAsync(string content, CancellationToken cancellationToken = default)
        => Task.FromResult(false);

    public Task<List<string>> SuggestTagsAsync(string title, string body, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<string>());
}

