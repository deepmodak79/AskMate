using DeepOverflow.Domain.Entities;

namespace DeepOverflow.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern for transaction management
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IQuestionRepository Questions { get; }
    IAnswerRepository Answers { get; }
    ICommentRepository Comments { get; }
    IUserRepository Users { get; }
    ITagRepository Tags { get; }
    IVoteRepository Votes { get; }
    IRepository<BadgeDefinition> BadgeDefinitions { get; }
    IRepository<UserBadge> UserBadges { get; }
    IRepository<ReputationHistory> ReputationHistory { get; }
    IRepository<Notification> Notifications { get; }
    IRepository<Flag> Flags { get; }
    IRepository<AuditLog> AuditLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
