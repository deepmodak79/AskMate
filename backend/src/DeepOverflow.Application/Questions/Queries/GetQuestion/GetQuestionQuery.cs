using MediatR;
using DeepOverflow.Application.Common;

namespace DeepOverflow.Application.Questions.Queries.GetQuestion;

/// <summary>
/// Query to get a question by ID or slug
/// </summary>
public class GetQuestionQuery : IRequest<Result<QuestionDetailDto>>
{
    public Guid? Id { get; set; }
    public string? Slug { get; set; }
    public bool IncrementViewCount { get; set; } = true;
}

public class QuestionDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    
    // Author
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public int AuthorReputation { get; set; }
    
    // Status
    public string Status { get; set; } = string.Empty;
    public Guid? AcceptedAnswerId { get; set; }
    
    // Metrics
    public int ViewCount { get; set; }
    public int VoteScore { get; set; }
    public int AnswerCount { get; set; }
    public int CommentCount { get; set; }
    public int BookmarkCount { get; set; }
    
    // Tags
    public List<TagDto> Tags { get; set; } = new();
    
    // Answers
    public List<AnswerDto> Answers { get; set; } = new();
    
    // Comments
    public List<CommentDto> Comments { get; set; } = new();
    
    // User interaction
    public int? CurrentUserVote { get; set; } // 1 = upvote, -1 = downvote, null = no vote
    public bool IsBookmarked { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanClose { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UsageCount { get; set; }
}

public class AnswerDto
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public int VoteScore { get; set; }
    public int CommentCount { get; set; }
    
    // Author
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public string? AuthorAvatarUrl { get; set; }
    public int AuthorReputation { get; set; }
    
    // Comments
    public List<CommentDto> Comments { get; set; } = new();
    
    // User interaction
    public int? CurrentUserVote { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public bool CanAccept { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public int VoteScore { get; set; }
    
    // Author
    public Guid AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public string AuthorDisplayName { get; set; } = string.Empty;
    public int AuthorReputation { get; set; }
    
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
