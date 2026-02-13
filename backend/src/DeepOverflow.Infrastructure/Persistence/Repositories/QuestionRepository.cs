using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Enums;
using DeepOverflow.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.Infrastructure.Persistence.Repositories;

public sealed class QuestionRepository : EfRepository<Question>, IQuestionRepository
{
    public QuestionRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    private IQueryable<Question> QueryWithDetails()
    {
        return DbContext.Questions
            .Include(q => q.Author)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .Include(q => q.Answers).ThenInclude(a => a.Author);
    }

    public override async Task<Question?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
    }

    public async Task<Question?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await QueryWithDetails()
            .FirstOrDefaultAsync(q => q.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetByAuthorIdAsync(Guid authorId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbContext.Questions
            .AsNoTracking()
            .Where(q => q.AuthorId == authorId)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetByTagAsync(string tagName, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var normalized = tagName.ToLowerInvariant().Trim();

        return await DbContext.Questions
            .AsNoTracking()
            .Where(q => q.QuestionTags.Any(qt => qt.Tag.Name == normalized))
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetUnansweredAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbContext.Questions
            .AsNoTracking()
            .Where(q => q.AnswerCount == 0)
            .OrderByDescending(q => q.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetByStatusAsync(QuestionStatus status, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbContext.Questions
            .AsNoTracking()
            .Where(q => q.Status == status)
            .OrderByDescending(q => q.LastActivityAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetHotQuestionsAsync(int count, CancellationToken cancellationToken = default)
    {
        return await DbContext.Questions
            .AsNoTracking()
            .OrderByDescending(q => q.VoteScore)
            .ThenByDescending(q => q.LastActivityAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Question>> GetRecentlyActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await DbContext.Questions
            .AsNoTracking()
            .OrderByDescending(q => q.LastActivityAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task IncrementViewCountAsync(Guid questionId, CancellationToken cancellationToken = default)
    {
        var question = await DbContext.Questions.FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);
        if (question == null) return;

        question.IncrementViewCount();
    }
}

