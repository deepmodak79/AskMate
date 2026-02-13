using DeepOverflow.Domain.Common;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Many-to-many relationship between Questions and Tags
/// </summary>
public class QuestionTag
{
    public Guid QuestionId { get; set; }
    public virtual Question Question { get; set; } = null!;

    public Guid TagId { get; set; }
    public virtual Tag Tag { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public QuestionTag()
    {
        CreatedAt = DateTime.UtcNow;
    }
}
