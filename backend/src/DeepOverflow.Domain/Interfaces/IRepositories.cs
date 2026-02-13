using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Enums;

namespace DeepOverflow.Domain.Interfaces;

public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetByAuthorIdAsync(Guid authorId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetByTagAsync(string tagName, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetUnansweredAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetByStatusAsync(QuestionStatus status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetHotQuestionsAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Question>> GetRecentlyActiveAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(Guid questionId, CancellationToken cancellationToken = default);
}

public interface IAnswerRepository : IRepository<Answer>
{
    Task<IReadOnlyList<Answer>> GetByQuestionIdAsync(Guid questionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Answer>> GetByAuthorIdAsync(Guid authorId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Answer?> GetAcceptedAnswerAsync(Guid questionId, CancellationToken cancellationToken = default);
}

public interface ICommentRepository : IRepository<Comment>
{
    Task<IReadOnlyList<Comment>> GetByTargetAsync(string targetType, Guid targetId, CancellationToken cancellationToken = default);
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetBySsoIdAsync(string ssoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetLeaderboardAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByDepartmentAsync(string department, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}

public interface ITagRepository : IRepository<Tag>
{
    Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> GetPopularTagsAsync(int count, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tag>> SearchTagsAsync(string searchTerm, int count, CancellationToken cancellationToken = default);
}

public interface IVoteRepository : IRepository<Vote>
{
    Task<Vote?> GetUserVoteAsync(Guid userId, string targetType, Guid targetId, CancellationToken cancellationToken = default);
    Task<int> GetVoteScoreAsync(string targetType, Guid targetId, CancellationToken cancellationToken = default);
}
