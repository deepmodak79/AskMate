using DeepOverflow.Domain.Common;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Entities;

/// <summary>
/// User entity representing RMES employees
/// </summary>
public class User : BaseEntity
{
    // Authentication
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public AuthProvider AuthProvider { get; set; }
    public string? SsoId { get; set; }

    // Profile
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Location { get; set; }
    public List<string> Skills { get; set; } = new();

    // Status
    public UserRole Role { get; set; }
    public int Reputation { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }

    // Security
    public DateTime? LastLoginAt { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    public DateTime? PasswordChangedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public virtual ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    public virtual ICollection<ReputationHistory> ReputationHistory { get; set; } = new List<ReputationHistory>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public User()
    {
        IsActive = true;
        IsEmailVerified = false;
        Role = UserRole.User;
        Reputation = 0;
        FailedLoginAttempts = 0;
    }

    // Domain methods
    public void AddReputation(int points)
    {
        Reputation = Math.Max(0, Reputation + points);
        UpdatedAt = DateTime.UtcNow;
    }

    public void PromoteToModerator()
    {
        if (Role == UserRole.User)
        {
            Role = UserRole.Moderator;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin(string? ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= 5)
        {
            LockedUntil = DateTime.UtcNow.AddMinutes(15);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLocked()
    {
        return LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;
    }
}
