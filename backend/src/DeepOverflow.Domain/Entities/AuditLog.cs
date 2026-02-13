using DeepOverflow.Domain.Common;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// Audit log for enterprise compliance
/// </summary>
public class AuditLog : BaseEntity
{
    // Who did it
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // What happened
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }

    // Details
    public string? Description { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }

    // Context
    public string? RequestId { get; set; }
    public string? SessionId { get; set; }
}
