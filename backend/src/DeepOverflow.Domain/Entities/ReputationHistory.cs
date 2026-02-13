using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Reputation history for tracking reputation changes
/// </summary>
public class ReputationHistory : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public ReputationAction Action { get; set; }
    public int Points { get; set; }

    // Context
    public Guid? RelatedQuestionId { get; set; }
    public Guid? RelatedAnswerId { get; set; }
    public Guid? RelatedUserId { get; set; }

    public string? Description { get; set; }

    public ReputationHistory()
    {
    }
}
