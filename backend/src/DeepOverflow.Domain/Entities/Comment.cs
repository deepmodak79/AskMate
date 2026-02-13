using DeepOverflow.Domain.Common;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Comment entity - for both questions and answers
/// </summary>
public class Comment : BaseEntity
{
    public string Body { get; set; } = string.Empty;

    // Polymorphic relationship
    public string TargetType { get; set; } = string.Empty; // "Question" or "Answer"
    public Guid TargetId { get; set; }

    // Ownership
    public Guid AuthorId { get; set; }
    public virtual User Author { get; set; } = null!;

    // Engagement
    public int VoteScore { get; set; }

    // Navigation properties
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();

    public Comment()
    {
        VoteScore = 0;
    }
}
