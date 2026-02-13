using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Interfaces;
using DeepOverflow.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace DeepOverflow.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        Questions = new QuestionRepository(dbContext);
        Answers = new AnswerRepository(dbContext);
        Comments = new CommentRepository(dbContext);
        Users = new UserRepository(dbContext);
        Tags = new TagRepository(dbContext);
        Votes = new VoteRepository(dbContext);

        BadgeDefinitions = new EfRepository<BadgeDefinition>(dbContext);
        UserBadges = new EfRepository<UserBadge>(dbContext);
        ReputationHistory = new EfRepository<ReputationHistory>(dbContext);
        Notifications = new EfRepository<Notification>(dbContext);
        Flags = new EfRepository<Flag>(dbContext);
        AuditLogs = new EfRepository<AuditLog>(dbContext);
    }

    public IQuestionRepository Questions { get; }
    public IAnswerRepository Answers { get; }
    public ICommentRepository Comments { get; }
    public IUserRepository Users { get; }
    public ITagRepository Tags { get; }
    public IVoteRepository Votes { get; }
    public IRepository<BadgeDefinition> BadgeDefinitions { get; }
    public IRepository<UserBadge> UserBadges { get; }
    public IRepository<ReputationHistory> ReputationHistory { get; }
    public IRepository<Notification> Notifications { get; }
    public IRepository<Flag> Flags { get; }
    public IRepository<AuditLog> AuditLogs { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null) return;
        _transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null) return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _dbContext.Dispose();
    }
}

