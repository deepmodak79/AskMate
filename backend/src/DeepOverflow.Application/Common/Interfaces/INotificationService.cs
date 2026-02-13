using DeepOverflow.Domain.Entities;

namespace DeepOverflow.Application.Common.Interfaces;

/// <summary>
/// Notification service interface
/// </summary>
public interface INotificationService
{
    Task NotifyAnswerReceivedAsync(Guid questionAuthorId, Guid answerId, CancellationToken cancellationToken = default);
    Task NotifyCommentReceivedAsync(Guid targetOwnerId, Guid commentId, CancellationToken cancellationToken = default);
    Task NotifyAnswerAcceptedAsync(Guid answerAuthorId, Guid answerId, CancellationToken cancellationToken = default);
    Task NotifyBadgeEarnedAsync(Guid userId, Guid badgeId, CancellationToken cancellationToken = default);
    Task NotifyMentionedAsync(Guid userId, string content, Guid relatedEntityId, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
