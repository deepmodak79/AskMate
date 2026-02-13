using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Badge definition - template for badges
/// </summary>
public class BadgeDefinition : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BadgeType BadgeType { get; set; }
    public BadgeCategory Category { get; set; }
    public string? IconUrl { get; set; }

    // Criteria as JSON for flexibility
    public string? CriteriaJson { get; set; }

    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }

    // Navigation properties
    public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();

    public BadgeDefinition()
    {
        IsActive = true;
        DisplayOrder = 0;
    }
}

/// <summary>
/// User badge - actual badge award to a user
/// </summary>
public class UserBadge : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid BadgeId { get; set; }
    public virtual BadgeDefinition Badge { get; set; } = null!;

    // Context (e.g., which question/answer earned it)
    public string? EarnedForType { get; set; }
    public Guid? EarnedForId { get; set; }

    public DateTime EarnedAt { get; set; }

    public UserBadge()
    {
        EarnedAt = DateTime.UtcNow;
    }
}
