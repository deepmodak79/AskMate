using DeepOverflow.Domain.Common;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Bookmark entity for users to save questions
/// </summary>
public class Bookmark : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public virtual Question Question { get; set; } = null!;

    public string? Notes { get; set; }
}
