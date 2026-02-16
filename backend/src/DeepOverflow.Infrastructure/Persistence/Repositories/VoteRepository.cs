using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Enums;
using DeepOverflow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.Infrastructure.Persistence.Repositories;

public sealed class VoteRepository : EfRepository<Vote>, IVoteRepository
{
    public VoteRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<Vote?> GetUserVoteAsync(Guid userId, string targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<VoteTargetType>(targetType, ignoreCase: true, out var parsed))
        {
            return null;
        }

        return await DbContext.Votes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UserId == userId && v.TargetType == parsed && v.TargetId == targetId, cancellationToken);
    }

    public async Task<Vote?> FindUserVoteAsync(Guid userId, VoteTargetType targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Votes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.TargetType == targetType && v.TargetId == targetId, cancellationToken);
    }

    public async Task<int> GetVoteScoreAsync(string targetType, Guid targetId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<VoteTargetType>(targetType, ignoreCase: true, out var parsed))
        {
            return 0;
        }

        // Upvote = +1, Downvote = -1
        return await DbContext.Votes
            .AsNoTracking()
            .Where(v => v.TargetType == parsed && v.TargetId == targetId)
            .SumAsync(v => v.VoteType == VoteType.Upvote ? 1 : -1, cancellationToken);
    }
}

