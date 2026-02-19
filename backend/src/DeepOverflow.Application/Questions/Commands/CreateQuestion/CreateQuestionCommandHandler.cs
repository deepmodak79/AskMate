using MediatR;
using DeepOverflow.Application.Common;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Entities;
using DeepOverflow.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DeepOverflow.Application.Questions.Commands.CreateQuestion;

/// <summary>
/// Handler for CreateQuestionCommand
/// </summary>
public class CreateQuestionCommandHandler : IRequestHandler<CreateQuestionCommand, Result<CreateQuestionResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISearchService _searchService;
    private readonly IAIService _aiService;
    private readonly ILogger<CreateQuestionCommandHandler> _logger;

    public CreateQuestionCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ISearchService searchService,
        IAIService aiService,
        ILogger<CreateQuestionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _searchService = searchService;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<Result<CreateQuestionResponse>> Handle(
        CreateQuestionCommand request, 
        CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
            {
                return Result<CreateQuestionResponse>.Failure("User must be authenticated");
            }

            var userId = _currentUserService.UserId.Value;
            var author = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (author == null)
            {
                _logger.LogWarning("User {UserId} from JWT not found in database (e.g. DB was reset). Ask user to re-login.", userId);
                return Result<CreateQuestionResponse>.Failure("Your account was not found. Please log out and log in again.");
            }

            if (string.IsNullOrWhiteSpace(request.Title))
                return Result<CreateQuestionResponse>.Failure("Title is required");
            if (request.Title.Trim().Length < 10)
                return Result<CreateQuestionResponse>.Failure("Title must be at least 10 characters");
            if (string.IsNullOrWhiteSpace(request.Body))
                return Result<CreateQuestionResponse>.Failure("Body is required");
            if (request.Body.Trim().Length < 20)
                return Result<CreateQuestionResponse>.Failure("Body must be at least 20 characters");

            // Check for spam
            var isSpam = await _aiService.IsSpamAsync($"{request.Title}\n{request.Body}", cancellationToken);
            if (isSpam)
            {
                _logger.LogWarning("Spam detected in question by user {UserId}", _currentUserService.UserId);
                return Result<CreateQuestionResponse>.Failure("Content appears to be spam");
            }

            // Create slug from title
            var slug = GenerateSlug(request.Title);
            var baseSlug = slug;
            var counter = 1;

            // Ensure unique slug
            while (await _unitOfWork.Questions.GetBySlugAsync(slug, cancellationToken) != null)
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            // Create question entity
            var question = new Question
            {
                Title = request.Title,
                Slug = slug,
                Body = request.Body,
                AuthorId = _currentUserService.UserId.Value
            };

            // Process tags (deduplicate to avoid duplicate QuestionTag key violation)
            var uniqueTagNames = request.Tags?.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim().ToLowerInvariant()).Distinct().ToList() ?? new List<string>();
            if (uniqueTagNames.Count == 0)
            {
                return Result<CreateQuestionResponse>.Failure("At least one tag is required");
            }
            var tagEntities = await ProcessTagsAsync(uniqueTagNames, _currentUserService.UserId.Value, cancellationToken);
            
            foreach (var tag in tagEntities)
            {
                question.QuestionTags.Add(new QuestionTag
                {
                    Question = question,
                    Tag = tag
                });
                tag.IncrementUsage();
            }

            // Add question and save first (avoids circular dependency with Answer->Question->AcceptedAnswerId)
            await _unitOfWork.Questions.AddAsync(question, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // If user has a solution, create answer and link it (after Question is persisted)
            if (!string.IsNullOrWhiteSpace(request.Solution))
            {
                var solutionBody = request.Solution.Trim();
                var answer = new Domain.Entities.Answer
                {
                    QuestionId = question.Id,
                    Body = solutionBody,
                    AuthorId = _currentUserService.UserId.Value,
                    IsAccepted = true,
                    AcceptedAt = DateTime.UtcNow
                };
                await _unitOfWork.Answers.AddAsync(answer, cancellationToken);
                question.AcceptAnswer(answer.Id);
                question.AnswerCount = 1;
                question.LastActivityAt = DateTime.UtcNow;
                question.UpdatedAt = DateTime.UtcNow;
            }
            
            // Give reputation points for asking question
            if (author != null)
            {
                // No points for asking, but track in history
                var reputationHistory = new ReputationHistory
                {
                    UserId = author.Id,
                    Action = Domain.Enums.ReputationAction.QuestionUpvoted,
                    Points = 0,
                    RelatedQuestionId = question.Id,
                    Description = $"Asked question: {question.Title}"
                };
                await _unitOfWork.ReputationHistory.AddAsync(reputationHistory, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Index in ElasticSearch asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await _searchService.IndexQuestionAsync(
                        question.Id,
                        question.Title,
                        question.Body,
                        request.Tags ?? new List<string>(),
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to index question {QuestionId}", question.Id);
                }
            }, cancellationToken);

            // Find similar questions using AI (async, non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    var similar = await _aiService.DetectSimilarQuestionsAsync(
                        question.Title,
                        question.Body,
                        CancellationToken.None);
                    
                    if (similar.Any(s => s.ConfidenceScore > 0.85f))
                    {
                        _logger.LogInformation(
                            "Potential duplicate question detected for {QuestionId}", 
                            question.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to detect similar questions for {QuestionId}", question.Id);
                }
            }, cancellationToken);

            _logger.LogInformation(
                "Question created: {QuestionId} by user {UserId}",
                question.Id,
                _currentUserService.UserId);

            return Result<CreateQuestionResponse>.Success(new CreateQuestionResponse
            {
                Id = question.Id,
                Slug = question.Slug,
                Title = question.Title,
                CreatedAt = question.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating question: {Message}", ex.Message);
            var msg = ex.InnerException?.Message ?? ex.Message;
            if (msg.Contains("UNIQUE constraint") || msg.Contains("duplicate key"))
                return Result<CreateQuestionResponse>.Failure("A question with this title may already exist. Try a different title.");
            if (msg.Contains("FOREIGN KEY") || msg.Contains("constraint"))
                return Result<CreateQuestionResponse>.Failure("Invalid data. Please check your question, tags, and try again.");
            if (msg.Contains("readonly") || msg.Contains("access") || msg.Contains("permission"))
                return Result<CreateQuestionResponse>.Failure("Database write failed. Please try again later.");
            return Result<CreateQuestionResponse>.Failure("An error occurred while creating the question. Please ensure Title and Body are filled, and try again.");
        }
    }

    private async Task<List<Tag>> ProcessTagsAsync(
        List<string> tagNames, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        var tags = new List<Tag>();

        foreach (var tagName in tagNames)
        {
            var normalizedName = tagName.ToLowerInvariant().Trim();
            var existingTag = await _unitOfWork.Tags.GetByNameAsync(normalizedName, cancellationToken);

            if (existingTag != null)
            {
                tags.Add(existingTag);
            }
            else
            {
                // Create new tag (will need moderator approval if user rep < 1000)
                var newTag = new Tag
                {
                    Name = normalizedName,
                    CreatedBy = userId,
                    IsApproved = false // Will be approved by moderator
                };
                await _unitOfWork.Tags.AddAsync(newTag, cancellationToken);
                tags.Add(newTag);
            }
        }

        return tags;
    }

    private static string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant();
        
        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        
        // Replace spaces with hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        
        // Remove multiple consecutive hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        
        // Trim hyphens from start and end
        slug = slug.Trim('-');
        
        // Limit length
        if (slug.Length > 100)
        {
            slug = slug.Substring(0, 100).TrimEnd('-');
        }

        return slug;
    }
}
