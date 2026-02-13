using DeepOverflow.Application.Common.Interfaces;

namespace DeepOverflow.Infrastructure.Services;

public sealed class NoOpSearchService : ISearchService
{
    public Task IndexQuestionAsync(Guid questionId, string title, string body, List<string> tags, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task UpdateQuestionIndexAsync(Guid questionId, string title, string body, List<string> tags, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task DeleteQuestionIndexAsync(Guid questionId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<SearchResult> SearchQuestionsAsync(string query, SearchFilters filters, int page, int pageSize, CancellationToken cancellationToken = default)
        => Task.FromResult(new SearchResult());

    public Task<List<SimilarQuestion>> FindSimilarQuestionsAsync(string title, string body, int count = 5, CancellationToken cancellationToken = default)
        => Task.FromResult(new List<SimilarQuestion>());
}

