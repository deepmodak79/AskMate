using DeepOverflow.Application.Common;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Enums;
using DeepOverflow.Domain.Interfaces;
using MediatR;

namespace DeepOverflow.Application.Questions.Commands.VoteQuestion;

public sealed class VoteQuestionCommandHandler : IRequestHandler<VoteQuestionCommand, Result<VoteQuestionResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public VoteQuestionCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<VoteQuestionResponse>> Handle(VoteQuestionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            return Result<VoteQuestionResponse>.Failure("You must be logged in to vote.");

        var voteType = string.Equals(request.VoteType, "downvote", StringComparison.OrdinalIgnoreCase)
            ? VoteType.Downvote
            : VoteType.Upvote;

        var question = await _unitOfWork.Questions.GetByIdAsync(request.QuestionId, cancellationToken);
        if (question == null)
            return Result<VoteQuestionResponse>.Failure("Question not found.");

        var existing = await _unitOfWork.Votes.FindUserVoteAsync(userId.Value, VoteTargetType.Question, request.QuestionId, cancellationToken);

        if (existing != null)
        {
            if (existing.VoteType == voteType)
            {
                // Same vote again: do nothing (one vote per user, no toggle-off)
                var currentScore = await _unitOfWork.Votes.GetVoteScoreAsync("Question", request.QuestionId, cancellationToken);
                return Result<VoteQuestionResponse>.Success(new VoteQuestionResponse { VoteScore = currentScore });
            }
            existing.VoteType = voteType;
            existing.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Votes.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            var vote = new Vote
            {
                UserId = userId.Value,
                TargetType = VoteTargetType.Question,
                TargetId = request.QuestionId,
                VoteType = voteType
            };
            await _unitOfWork.Votes.AddAsync(vote, cancellationToken);
        }

        var voteScore = await _unitOfWork.Votes.GetVoteScoreAsync("Question", request.QuestionId, cancellationToken);
        question.VoteScore = voteScore;
        question.UpdateActivity();
        await _unitOfWork.Questions.UpdateAsync(question, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VoteQuestionResponse>.Success(new VoteQuestionResponse { VoteScore = voteScore });
    }
}
