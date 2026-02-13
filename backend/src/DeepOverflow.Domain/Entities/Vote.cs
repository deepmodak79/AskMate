using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Vote entity - polymorphic voting for questions, answers, and comments
/// </summary>
public class Vote : BaseEntity
{
    // Polymorphic relationship
    public VoteTargetType TargetType { get; set; }
    public Guid TargetId { get; set; }

    // Vote details
    public VoteType VoteType { get; set; }
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Vote()
    {
    }
}
