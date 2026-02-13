using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Enums;
using DeepOverflow.Domain.Interfaces;

namespace DeepOverflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { error = "Email and password are required" });

        var user = await _unitOfWork.Users.GetByEmailAsync(request.Email.Trim(), HttpContext.RequestAborted);
        if (user == null || user.PasswordHash == null)
            return Unauthorized(new { error = "Invalid credentials" });

        if (user.IsLocked())
            return Unauthorized(new { error = "Account is temporarily locked. Try again later." });

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(HttpContext.RequestAborted);
            return Unauthorized(new { error = "Invalid credentials" });
        }

        user.RecordLogin(HttpContext.Connection.RemoteIpAddress?.ToString());
        await _unitOfWork.SaveChangesAsync(HttpContext.RequestAborted);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();

        return Ok(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString(),
                Reputation = user.Reputation,
                AvatarUrl = user.AvatarUrl,
                Department = user.Department
            }
        });
    }

    /// <summary>
    /// SSO authentication
    /// </summary>
    [HttpPost("sso")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> SsoLogin([FromBody] SsoLoginRequest request)
    {
        return Ok(new LoginResponse());
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        return Ok(new LoginResponse());
    }

    /// <summary>
    /// Logout
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout()
    {
        return Ok();
    }

    /// <summary>
    /// Register new user (if enabled)
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponse), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.DisplayName) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Email, username, displayName and password are required" });
        }

        var email = request.Email.Trim();
        var username = request.Username.Trim();

        if (await _unitOfWork.Users.EmailExistsAsync(email, HttpContext.RequestAborted))
            return BadRequest(new { error = "Email already exists" });

        if (await _unitOfWork.Users.UsernameExistsAsync(username, HttpContext.RequestAborted))
            return BadRequest(new { error = "Username already exists" });

        var user = new DeepOverflow.Domain.Entities.User
        {
            Email = email,
            Username = username,
            DisplayName = request.DisplayName.Trim(),
            Department = string.IsNullOrWhiteSpace(request.Department) ? null : request.Department.Trim(),
            AuthProvider = AuthProvider.Email,
            Role = UserRole.User,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsEmailVerified = true // dev default; wire real verification later
        };

        await _unitOfWork.Users.AddAsync(user, HttpContext.RequestAborted);
        await _unitOfWork.SaveChangesAsync(HttpContext.RequestAborted);

        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Username, user.Email, user.Role.ToString());
        var refreshToken = _tokenService.GenerateRefreshToken();

        return CreatedAtAction(nameof(Login), new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role.ToString(),
                Reputation = user.Reputation,
                AvatarUrl = user.AvatarUrl,
                Department = user.Department
            }
        });
    }

    /// <summary>
    /// Verify email
    /// </summary>
    [HttpGet("verify-email")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        return Ok(new { message = "Email verified successfully" });
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class SsoLoginRequest
{
    public string Token { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int Reputation { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Department { get; set; }
}
