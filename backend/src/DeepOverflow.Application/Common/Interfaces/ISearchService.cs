namespace DeepOverflow.Application.Common.Interfaces;

/// <summary>
/// Search service interface for ElasticSearch integration
/// </summary>
public interface ISearchService
{
    Task IndexQuestionAsync(Guid questionId, string title, string body, List<string> tags, CancellationToken cancellationToken = default);
    Task UpdateQuestionIndexAsync(Guid questionId, string title, string body, List<string> tags, CancellationToken cancellationToken = default);
    Task DeleteQuestionIndexAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<SearchResult> SearchQuestionsAsync(string query, SearchFilters filters, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<SimilarQuestion>> FindSimilarQuestionsAsync(string title, string body, int count = 5, CancellationToken cancellationToken = default);
}

public class SearchResult
{
    public List<SearchResultItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public Dictionary<string, long> TagFacets { get; set; } = new();
}

public class SearchResultItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BodyExcerpt { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public int VoteScore { get; set; }
    public int AnswerCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SimilarQuestion
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public float SimilarityScore { get; set; }
}

public class SearchFilters
{
    public List<string> Tags { get; set; } = new();
    public bool? HasAcceptedAnswer { get; set; }
    public bool? IsUnanswered { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? MinScore { get; set; }
    public string? SortBy { get; set; } // "relevance", "votes", "date", "activity"
}
