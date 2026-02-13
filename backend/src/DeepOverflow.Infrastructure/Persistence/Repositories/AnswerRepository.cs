using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.Infrastructure.Persistence.Repositories;

public sealed class AnswerRepository : EfRepository<Answer>, IAnswerRepository
{
    public AnswerRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public override async Task<Answer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Answers
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Answer>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Answers
            .AsNoTracking()
            .Include(a => a.Author)
            .Where(a => a.QuestionId == questionId)
            .OrderByDescending(a => a.IsAccepted)
            .ThenByDescending(a => a.VoteScore)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Answer>> GetByAuthorIdAsync(Guid authorId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbContext.Answers
            .AsNoTracking()
            .Where(a => a.AuthorId == authorId)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Answer?> GetAcceptedAnswerAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Answers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.QuestionId == questionId && a.IsAccepted, cancellationToken);
    }
}

