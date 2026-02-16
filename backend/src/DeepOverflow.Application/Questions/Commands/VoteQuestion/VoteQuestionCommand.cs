using MediatR;
using DeepOverflow.Application.Common;

namespace DeepOverflow.Application.Questions.Commands.VoteQuestion;

public class VoteQuestionCommand : IRequest<Result<VoteQuestionResponse>>
{
    public Guid QuestionId { get; set; }
    public string VoteType { get; set; } = "upvote"; // "upvote" or "downvote"
}

public class VoteQuestionResponse
{
    public int VoteScore { get; set; }
}
