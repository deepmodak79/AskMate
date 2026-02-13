using System.Security.Claims;
using DeepOverflow.Application.Common.Interfaces;

namespace DeepOverflow.API.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var user = User;
            if (user?.Identity?.IsAuthenticated != true) return null;

            var sub = user.FindFirst("sub")?.Value;
            if (Guid.TryParse(sub, out var id)) return id;

            var nameId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(nameId, out id) ? id : null;
        }
    }

    public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value ?? User?.FindFirst("unique_name")?.Value;
    public string? Email => User?.FindFirst(ClaimTypes.Email)?.Value ?? User?.FindFirst("email")?.Value;
    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}

