namespace DeepOverflow.Application.Common.Interfaces;

/// <summary>
/// Current user service for accessing authenticated user information
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
}
