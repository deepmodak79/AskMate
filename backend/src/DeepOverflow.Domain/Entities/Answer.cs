using DeepOverflow.Domain.Common;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Answer entity
/// </summary>
public class Answer : BaseEntity
{
    public Guid QuestionId { get; set; }
    public virtual Question Question { get; set; } = null!;

    public string Body { get; set; } = string.Empty;

    // Ownership
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;

    // Status
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }

    // Engagement
    public int VoteScore { get; set; }
    public int CommentCount { get; set; }

    // Navigation properties
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public virtual ICollection<EditHistory> EditHistories { get; set; } = new List<EditHistory>();
    public virtual ICollection<Flag> Flags { get; set; } = new List<Flag>();

    public Answer()
    {
        IsAccepted = false;
        VoteScore = 0;
        CommentCount = 0;
    }

    // Domain methods
    public void Accept()
    {
        IsAccepted = true;
        AcceptedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unaccept()
    {
        IsAccepted = false;
        AcceptedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
