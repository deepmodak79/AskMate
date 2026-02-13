using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.Infrastructure.Persistence.Repositories;

public sealed class TagRepository : EfRepository<Tag>, ITagRepository
{
    public TagRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalized = name.ToLowerInvariant().Trim();
        return await DbContext.Tags.FirstOrDefaultAsync(t => t.Name == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await DbContext.Tags
            .AsNoTracking()
            .Where(t => t.IsApproved)
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> SearchTagsAsync(string searchTerm, int count, CancellationToken cancellationToken = default)
    {
        var normalized = searchTerm.ToLowerInvariant().Trim();
        return await DbContext.Tags
            .AsNoTracking()
            .Where(t => t.Name.Contains(normalized))
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}

