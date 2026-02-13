using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Question entity - the core content type
/// </summary>
public class Question : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;

    // Ownership
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;

    // Status
    public QuestionStatus Status { get; set; }
    public Guid? AcceptedAnswerId { get; set; }
    public virtual Answer? AcceptedAnswer { get; set; }
    public Guid? DuplicateOfQuestionId { get; set; }
    public virtual Question? DuplicateOfQuestion { get; set; }

    // Engagement metrics
    public int ViewCount { get; set; }
    public int VoteScore { get; set; }
    public int AnswerCount { get; set; }
    public int CommentCount { get; set; }
    public int BookmarkCount { get; set; }

    // Moderation
    public bool IsLocked { get; set; }
    public Guid? LockedBy { get; set; }
    public DateTime? LockedAt { get; set; }
    public string? LockReason { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime? FeaturedUntil { get; set; }

    // Activity tracking
    public DateTime LastActivityAt { get; set; }

    // Navigation properties
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public virtual ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public virtual ICollection<EditHistory> EditHistories { get; set; } = new List<EditHistory>();
    public virtual ICollection<Flag> Flags { get; set; } = new List<Flag>();

    public Question()
    {
        Status = QuestionStatus.Open;
        LastActivityAt = DateTime.UtcNow;
        ViewCount = 0;
        VoteScore = 0;
        AnswerCount = 0;
        CommentCount = 0;
        BookmarkCount = 0;
        IsLocked = false;
        IsFeatured = false;
    }

    // Domain methods
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateActivity()
    {
        LastActivityAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AcceptAnswer(Guid answerId)
    {
        AcceptedAnswerId = answerId;
        Status = QuestionStatus.Resolved;
        UpdateActivity();
    }

    public void Lock(Guid moderatorId, string reason)
    {
        IsLocked = true;
        LockedBy = moderatorId;
        LockedAt = DateTime.UtcNow;
        LockReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unlock()
    {
        IsLocked = false;
        LockedBy = null;
        LockedAt = null;
        LockReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDuplicate(Guid duplicateOfId)
    {
        Status = QuestionStatus.Duplicate;
        DuplicateOfQuestionId = duplicateOfId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = QuestionStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reopen()
    {
        Status = QuestionStatus.Open;
        UpdatedAt = DateTime.UtcNow;
    }
}
