using MediatR;
using DeepOverflow.Application.Common;

namespace DeepOverflow.Application.Answers.Commands.VoteAnswer;

public class VoteAnswerCommand : IRequest<Result<VoteAnswerResponse>>
{
    public Guid AnswerId { get; set; }
    public string VoteType { get; set; } = "upvote"; // "upvote" or "downvote"
}

public class VoteAnswerResponse
{
    public int VoteScore { get; set; }
}
