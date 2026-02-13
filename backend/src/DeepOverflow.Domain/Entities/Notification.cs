using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Notification entity for user notifications
/// </summary>
public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public NotificationType NotificationType { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }

    // Context
    public Guid? RelatedQuestionId { get; set; }
    public Guid? RelatedAnswerId { get; set; }
    public Guid? RelatedUserId { get; set; }

    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }

    public Notification()
    {
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
