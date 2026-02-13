using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Flag entity for moderation
/// </summary>
public class Flag : BaseEntity
{
    // Polymorphic relationship
    public FlagTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    // Flag details
    public FlagReason Reason { get; set; }
    public string? Description { get; set; }
    public Guid FlaggerId { get; set; }
    public virtual User Flagger { get; set; } = null!;

    // Resolution
    public FlagStatus Status { get; set; }
    public Guid? ReviewedBy { get; set; }
    public virtual User? Reviewer { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ModeratorNote { get; set; }

    public Flag()
    {
        Status = FlagStatus.Pending;
    }

    public void Resolve(Guid reviewerId, bool isHelpful, string? note = null)
    {
        Status = isHelpful ? FlagStatus.Helpful : FlagStatus.Declined;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;
        ModeratorNote = note;
        UpdatedAt = DateTime.UtcNow;
    }
}
