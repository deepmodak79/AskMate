using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Tag entity for categorizing questions
/// </summary>
public class Tag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TagCategory Category { get; set; }
    public int UsageCount { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsApproved { get; set; }

    // Navigation properties
    public virtual ICollection<QuestionTag> QuestionTags { get; set; } = new List<QuestionTag>();

    public Tag()
    {
        UsageCount = 0;
        IsApproved = false;
        Category = TagCategory.General;
    }

    public void IncrementUsage()
    {
        UsageCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementUsage()
    {
        if (UsageCount > 0)
        {
            UsageCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
