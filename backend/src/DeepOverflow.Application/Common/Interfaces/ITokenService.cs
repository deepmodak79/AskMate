namespace DeepOverflow.Application.Common.Interfaces;

/// <summary>
/// JWT token service for authentication
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string username, string email, string role);
    string GenerateRefreshToken();
    Task<bool> ValidateTokenAsync(string token);
    Guid? GetUserIdFromToken(string token);
}
