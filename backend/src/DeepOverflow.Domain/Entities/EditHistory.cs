using DeepOverflow.Domain.Common;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Edit history for tracking changes to questions and answers
/// </summary>
public class EditHistory : BaseEntity
{
    // Polymorphic relationship
    public string TargetType { get; set; } = string.Empty; // "Question", "Answer", "Tag"
    public Guid TargetId { get; set; }

    // Edit details
    public Guid EditorId { get; set; }
    public virtual User Editor { get; set; } = null!;
    public string? EditSummary { get; set; }

    // Content snapshots
    public string? BeforeTitle { get; set; }
    public string? AfterTitle { get; set; }
    public string? BeforeBody { get; set; }
    public string? AfterBody { get; set; }
    public List<Guid>? BeforeTags { get; set; }
    public List<Guid>? AfterTags { get; set; }

    // Approval (for low-rep users)
    public bool NeedsApproval { get; set; }
    public bool? IsApproved { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public EditHistory()
    {
        NeedsApproval = false;
    }
}
