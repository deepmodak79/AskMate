using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : EfRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLowerInvariant().Trim();
        return await DbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.ToLowerInvariant().Trim();
        return await DbContext.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized, cancellationToken);
    }

    public async Task<User?> GetBySsoIdAsync(string ssoId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Users.FirstOrDefaultAsync(u => u.SsoId == ssoId, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetLeaderboardAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbContext.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .OrderByDescending(u => u.Reputation)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByDepartmentAsync(string department, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalized = department.ToLowerInvariant().Trim();
        return await DbContext.Users
            .AsNoTracking()
            .Where(u => u.Department != null && u.Department.ToLower() == normalized)
            .OrderByDescending(u => u.Reputation)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = email.ToLowerInvariant().Trim();
        return await DbContext.Users.AnyAsync(u => u.Email.ToLower() == normalized, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalized = username.ToLowerInvariant().Trim();
        return await DbContext.Users.AnyAsync(u => u.Username.ToLower() == normalized, cancellationToken);
    }
}

