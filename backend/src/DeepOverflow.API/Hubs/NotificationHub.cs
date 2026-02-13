using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace DeepOverflow.API.Hubs;

/// <summary>
/// SignalR hub for real-time notifications
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            // Add user to their personal group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} connected to notification hub", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value;
        
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("User {UserId} disconnected from notification hub", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Client subscribes to question updates
    /// </summary>
    public async Task SubscribeToQuestion(string questionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"question_{questionId}");
        _logger.LogInformation("User subscribed to question {QuestionId}", questionId);
    }

    /// <summary>
    /// Client unsubscribes from question updates
    /// </summary>
    public async Task UnsubscribeFromQuestion(string questionId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"question_{questionId}");
        _logger.LogInformation("User unsubscribed from question {QuestionId}", questionId);
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async Task MarkNotificationAsRead(string notificationId)
    {
        // Implementation would update notification in database
        _logger.LogInformation("Notification {NotificationId} marked as read", notificationId);
    }
}

/// <summary>
/// Extension methods for sending notifications via SignalR
/// </summary>
public static class NotificationHubExtensions
{
    public static async Task SendNotificationToUser(
        this IHubContext<NotificationHub> hubContext,
        Guid userId,
        object notification)
    {
        await hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ReceiveNotification", notification);
    }

    public static async Task SendQuestionUpdate(
        this IHubContext<NotificationHub> hubContext,
        Guid questionId,
        object update)
    {
        await hubContext.Clients.Group($"question_{questionId}")
            .SendAsync("QuestionUpdated", update);
    }

    public static async Task SendVoteUpdate(
        this IHubContext<NotificationHub> hubContext,
        Guid targetId,
        string targetType,
        int newScore)
    {
        await hubContext.Clients.Group($"{targetType}_{targetId}")
            .SendAsync("VoteUpdated", new { targetId, targetType, newScore });
    }

    public static async Task SendNewAnswer(
        this IHubContext<NotificationHub> hubContext,
        Guid questionId,
        object answer)
    {
        await hubContext.Clients.Group($"question_{questionId}")
            .SendAsync("NewAnswer", answer);
    }

    public static async Task SendNewComment(
        this IHubContext<NotificationHub> hubContext,
        Guid targetId,
        string targetType,
        object comment)
    {
        await hubContext.Clients.Group($"{targetType}_{targetId}")
            .SendAsync("NewComment", comment);
    }
}
