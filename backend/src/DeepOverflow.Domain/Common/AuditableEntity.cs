namespace DeepOverflow.Domain.Common;

/// <summary>
/// Base audit entity with user tracking
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public Guid? CreatedBy { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
