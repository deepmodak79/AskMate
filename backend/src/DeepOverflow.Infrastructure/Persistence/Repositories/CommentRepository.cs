using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.Infrastructure.Persistence.Repositories;

public sealed class CommentRepository : EfRepository<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<IReadOnlyList<Comment>> GetByTargetAsync(string targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Comments
            .AsNoTracking()
            .Include(c => c.Author)
            .Where(c => c.TargetType == targetType && c.TargetId == targetId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}

