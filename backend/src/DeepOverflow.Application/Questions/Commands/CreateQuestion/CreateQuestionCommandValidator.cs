using FluentValidation;

namespace DeepOverflow.Application.Questions.Commands.CreateQuestion;

/// <summary>
/// Validator for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(10).WithMessage("Title must be at least 10 characters")
            .MaximumLength(300).WithMessage("Title cannot exceed 300 characters");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Question body is required")
            .MinimumLength(20).WithMessage("Question body must be at least 20 characters");

        RuleFor(x => x.Tags)
            .NotEmpty().WithMessage("At least one tag is required")
            .Must(tags => tags.Count >= 1 && tags.Count <= 5)
            .WithMessage("You must provide between 1 and 5 tags");

        RuleForEach(x => x.Tags)
            .NotEmpty().WithMessage("Tag cannot be empty")
            .MaximumLength(50).WithMessage("Tag cannot exceed 50 characters")
            .Matches("^[a-z0-9\\-]+$").WithMessage("Tags must contain only lowercase letters, numbers, and hyphens");
    }
}
