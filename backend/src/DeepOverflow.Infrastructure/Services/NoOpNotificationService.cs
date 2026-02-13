using DeepOverflow.Application.Common.Interfaces;

namespace DeepOverflow.Infrastructure.Services;

public sealed class NoOpNotificationService : INotificationService
{
    public Task NotifyAnswerReceivedAsync(Guid questionAuthorId, Guid answerId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task NotifyCommentReceivedAsync(Guid targetOwnerId, Guid commentId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task NotifyAnswerAcceptedAsync(Guid answerAuthorId, Guid answerId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task NotifyBadgeEarnedAsync(Guid userId, Guid badgeId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task NotifyMentionedAsync(Guid userId, string content, Guid relatedEntityId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult(0);
}

