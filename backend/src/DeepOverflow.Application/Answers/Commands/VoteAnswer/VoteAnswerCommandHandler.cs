using DeepOverflow.Application.Common;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Enums;
using DeepOverflow.Domain.Interfaces;
using MediatR;

namespace DeepOverflow.Application.Answers.Commands.VoteAnswer;

public sealed class VoteAnswerCommandHandler : IRequestHandler<VoteAnswerCommand, Result<VoteAnswerResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public VoteAnswerCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<VoteAnswerResponse>> Handle(VoteAnswerCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
            return Result<VoteAnswerResponse>.Failure("You must be logged in to vote.");

        var voteType = string.Equals(request.VoteType, "downvote", StringComparison.OrdinalIgnoreCase)
            ? VoteType.Downvote
            : VoteType.Upvote;

        var answer = await _unitOfWork.Answers.GetByIdAsync(request.AnswerId, cancellationToken);
        if (answer == null)
            return Result<VoteAnswerResponse>.Failure("Answer not found.");

        var existing = await _unitOfWork.Votes.FindUserVoteAsync(userId.Value, VoteTargetType.Answer, request.AnswerId, cancellationToken);

        if (existing != null)
        {
            if (existing.VoteType == voteType)
            {
                // Same vote again: do nothing (one vote per user, no toggle-off)
                var currentScore = await _unitOfWork.Votes.GetVoteScoreAsync("Answer", request.AnswerId, cancellationToken);
                return Result<VoteAnswerResponse>.Success(new VoteAnswerResponse { VoteScore = currentScore });
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
                TargetType = VoteTargetType.Answer,
                TargetId = request.AnswerId,
                VoteType = voteType
            };
            await _unitOfWork.Votes.AddAsync(vote, cancellationToken);
        }

        var voteScore = await _unitOfWork.Votes.GetVoteScoreAsync("Answer", request.AnswerId, cancellationToken);
        answer.VoteScore = voteScore;
        answer.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Answers.UpdateAsync(answer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<VoteAnswerResponse>.Success(new VoteAnswerResponse { VoteScore = voteScore });
    }
}
