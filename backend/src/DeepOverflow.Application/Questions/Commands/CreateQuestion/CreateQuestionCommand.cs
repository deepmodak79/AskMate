using MediatR;
using DeepOverflow.Application.Common;

namespace DeepOverflow.Application.Questions.Commands.CreateQuestion;

/// <summary>
/// Command to create a new question
/// </summary>
public class CreateQuestionCommand : IRequest<Result<CreateQuestionResponse>>
{
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    /// <summary>
    /// Optional: If the user already has a solution, they can share it with the question.
    /// The solution will be posted as an answer and auto-accepted.
    /// </summary>
    public string? Solution { get; set; }
}

public class CreateQuestionResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
