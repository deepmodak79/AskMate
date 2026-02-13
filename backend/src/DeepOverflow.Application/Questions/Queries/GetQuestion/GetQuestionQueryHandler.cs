using DeepOverflow.Application.Common;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DeepOverflow.Application.Questions.Queries.GetQuestion;

public sealed class GetQuestionQueryHandler : IRequestHandler<GetQuestionQuery, Result<QuestionDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<GetQuestionQueryHandler> _logger;

    public GetQuestionQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<GetQuestionQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<QuestionDetailDto>> Handle(GetQuestionQuery request, CancellationToken cancellationToken)
    {
        if (!request.Id.HasValue && string.IsNullOrWhiteSpace(request.Slug))
        {
            return Result<QuestionDetailDto>.Failure("Either Id or Slug must be provided");
        }

        var question = request.Id.HasValue
            ? await _unitOfWork.Questions.GetByIdAsync(request.Id.Value, cancellationToken)
            : await _unitOfWork.Questions.GetBySlugAsync(request.Slug!, cancellationToken);

        if (question == null)
        {
            return Result<QuestionDetailDto>.Failure("Question not found");
        }

        if (request.IncrementViewCount)
        {
            question.IncrementViewCount();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var currentUserId = _currentUser.UserId;
        var currentRole = _currentUser.Role ?? string.Empty;
        var isOwner = currentUserId.HasValue && currentUserId.Value == question.AuthorId;
        var isModerator = string.Equals(currentRole, "Moderator", StringComparison.OrdinalIgnoreCase)
            || string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase);

        var dto = new QuestionDetailDto
        {
            Id = question.Id,
            Title = question.Title,
            Slug = question.Slug,
            Body = question.Body,

            AuthorId = question.AuthorId,
            AuthorUsername = question.Author?.Username ?? string.Empty,
            AuthorDisplayName = question.Author?.DisplayName ?? string.Empty,
            AuthorAvatarUrl = question.Author?.AvatarUrl,
            AuthorReputation = question.Author?.Reputation ?? 0,

            Status = question.Status.ToString(),
            AcceptedAnswerId = question.AcceptedAnswerId,

            ViewCount = question.ViewCount,
            VoteScore = question.VoteScore,
            AnswerCount = question.AnswerCount,
            CommentCount = question.CommentCount,
            BookmarkCount = question.BookmarkCount,

            Tags = question.QuestionTags
                .Select(qt => qt.Tag)
                .Where(t => t != null)
                .Select(t => new TagDto
                {
                    Id = t!.Id,
                    Name = t.Name,
                    Description = t.Description,
                    UsageCount = t.UsageCount
                })
                .OrderBy(t => t.Name)
                .ToList(),

            Answers = question.Answers
                .OrderByDescending(a => a.IsAccepted)
                .ThenByDescending(a => a.VoteScore)
                .ThenBy(a => a.CreatedAt)
                .Select(a => new AnswerDto
                {
                    Id = a.Id,
                    Body = a.Body,
                    IsAccepted = a.IsAccepted,
                    VoteScore = a.VoteScore,
                    CommentCount = a.CommentCount,

                    AuthorId = a.AuthorId,
                    AuthorUsername = a.Author?.Username ?? string.Empty,
                    AuthorDisplayName = a.Author?.DisplayName ?? string.Empty,
                    AuthorAvatarUrl = a.Author?.AvatarUrl,
                    AuthorReputation = a.Author?.Reputation ?? 0,

                    Comments = a.Comments
                        .OrderBy(c => c.CreatedAt)
                        .Select(c => new CommentDto
                        {
                            Id = c.Id,
                            Body = c.Body,
                            VoteScore = c.VoteScore,
                            AuthorId = c.AuthorId,
                            AuthorUsername = c.Author?.Username ?? string.Empty,
                            AuthorDisplayName = c.Author?.DisplayName ?? string.Empty,
                            AuthorReputation = c.Author?.Reputation ?? 0,
                            CanEdit = currentUserId.HasValue && currentUserId.Value == c.AuthorId,
                            CanDelete = currentUserId.HasValue && (currentUserId.Value == c.AuthorId || isModerator),
                            CreatedAt = c.CreatedAt,
                            UpdatedAt = c.UpdatedAt
                        })
                        .ToList(),

                    CurrentUserVote = null,
                    CanEdit = currentUserId.HasValue && (currentUserId.Value == a.AuthorId || isModerator),
                    CanDelete = currentUserId.HasValue && (currentUserId.Value == a.AuthorId || isModerator),
                    CanAccept = isOwner,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToList(),

            Comments = question.Comments
                .OrderBy(c => c.CreatedAt)
                .Select(c => new CommentDto
                {
                    Id = c.Id,
                    Body = c.Body,
                    VoteScore = c.VoteScore,
                    AuthorId = c.AuthorId,
                    AuthorUsername = c.Author?.Username ?? string.Empty,
                    AuthorDisplayName = c.Author?.DisplayName ?? string.Empty,
                    AuthorReputation = c.Author?.Reputation ?? 0,
                    CanEdit = currentUserId.HasValue && currentUserId.Value == c.AuthorId,
                    CanDelete = currentUserId.HasValue && (currentUserId.Value == c.AuthorId || isModerator),
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToList(),

            CurrentUserVote = null,
            IsBookmarked = false,
            CanEdit = isOwner || isModerator,
            CanDelete = isOwner || isModerator,
            CanClose = isModerator,

            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            LastActivityAt = question.LastActivityAt
        };

        _logger.LogInformation("Fetched question {QuestionId}", question.Id);
        return Result<QuestionDetailDto>.Success(dto);
    }
}

