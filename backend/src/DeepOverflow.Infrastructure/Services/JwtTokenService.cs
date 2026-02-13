using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DeepOverflow.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DeepOverflow.Infrastructure.Services;

public sealed class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(Guid userId, string username, string email, string role)
    {
        var jwt = _configuration.GetSection("JWT");
        var secret = jwt["Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured");
        var issuer = jwt["Issuer"] ?? "DeepOverflow";
        var audience = jwt["Audience"] ?? "DeepOverflowUsers";
        var expiryMinutes = int.TryParse(jwt["AccessTokenExpiryMinutes"], out var minutes) ? minutes : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            // "sub" is used in NotificationHub
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var jwt = _configuration.GetSection("JWT");
            var secret = jwt["Secret"] ?? throw new InvalidOperationException("JWT:Secret not configured");
            var issuer = jwt["Issuer"] ?? "DeepOverflow";
            var audience = jwt["Audience"] ?? "DeepOverflowUsers";

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.Zero
            };

            new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var sub = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            if (Guid.TryParse(sub, out var id)) return id;

            var nameId = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(nameId, out id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}

