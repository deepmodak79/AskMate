using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DeepOverflow.Application.Questions.Commands.CreateQuestion;
using DeepOverflow.Application.Questions.Commands.VoteQuestion;
using DeepOverflow.Application.Questions.Queries.GetQuestion;
using DeepOverflow.Application.Common;
using DeepOverflow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(IMediator mediator, ApplicationDbContext dbContext, ILogger<QuestionsController> logger)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated list of questions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<QuestionDetailDto>), 200)]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "newest",
        [FromQuery] string? q = null,
        [FromQuery] string[]? tags = null,
        [FromQuery] string? status = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var query = _dbContext.Questions
            .AsNoTracking()
            .Include(q => q.Author)
            .Include(q => q.QuestionTags).ThenInclude(qt => qt.Tag)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = $"%{q.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Title, term) ||
                EF.Functions.Like(x.Body, term));
        }

        if (tags is { Length: > 0 })
        {
            var normalizedTags = tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLowerInvariant())
                .Distinct()
                .ToArray();

            if (normalizedTags.Length > 0)
            {
                query = query.Where(q => q.QuestionTags.Any(qt => normalizedTags.Contains(qt.Tag.Name)));
            }
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<DeepOverflow.Domain.Enums.QuestionStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(q => q.Status == parsedStatus);
        }

        query = (sortBy ?? "newest").ToLowerInvariant() switch
        {
            "votes" => query.OrderByDescending(q => q.VoteScore).ThenByDescending(q => q.LastActivityAt),
            "active" => query.OrderByDescending(q => q.LastActivityAt),
            "unanswered" => query.Where(q => q.AnswerCount == 0).OrderByDescending(q => q.CreatedAt),
            _ => query.OrderByDescending(q => q.CreatedAt)
        };

        var totalCount = await query.CountAsync(HttpContext.RequestAborted);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new QuestionListItemDto
            {
                Id = q.Id,
                Title = q.Title,
                Slug = q.Slug,
                Status = q.Status.ToString(),
                ViewCount = q.ViewCount,
                VoteScore = q.VoteScore,
                AnswerCount = q.AnswerCount,
                CommentCount = q.CommentCount,
                BookmarkCount = q.BookmarkCount,
                Author = new QuestionAuthorDto
                {
                    Id = q.AuthorId,
                    Username = q.Author.Username,
                    DisplayName = q.Author.DisplayName,
                    Reputation = q.Author.Reputation,
                    AvatarUrl = q.Author.AvatarUrl
                },
                Tags = q.QuestionTags.Select(qt => new TagDto
                {
                    Id = qt.Tag.Id,
                    Name = qt.Tag.Name,
                    Description = qt.Tag.Description,
                    UsageCount = qt.Tag.UsageCount
                }).ToList(),
                CreatedAt = q.CreatedAt,
                UpdatedAt = q.UpdatedAt,
                LastActivityAt = q.LastActivityAt
            })
            .ToListAsync(HttpContext.RequestAborted);

        return Ok(new PaginatedResult<QuestionListItemDto>(items, totalCount, page, pageSize));
    }

    /// <summary>
    /// Get question by ID or slug
    /// </summary>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType(typeof(QuestionDetailDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetQuestion(string idOrSlug)
    {
        var query = new GetQuestionQuery();
        
        if (Guid.TryParse(idOrSlug, out var id))
        {
            query.Id = id;
        }
        else
        {
            query.Slug = idOrSlug;
        }

        var result = await _mediator.Send(query);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.ErrorMessage);
        }

        return Ok(result.Data);
    }

    /// <summary>
    /// Create a new question
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateQuestionResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(
            nameof(GetQuestion), 
            new { idOrSlug = result.Data!.Slug }, 
            result.Data);
    }

    /// <summary>
    /// Update a question
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionCommand command)
    {
        command.Id = id;
        // Implementation would follow similar pattern
        return Ok();
    }

    /// <summary>
    /// Delete a question
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        // Implementation would follow similar pattern
        return NoContent();
    }

    /// <summary>
    /// Vote on a question
    /// </summary>
    [HttpPost("{id}/vote")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> VoteQuestion(Guid id, [FromBody] VoteRequest request)
    {
        var result = await _mediator.Send(new VoteQuestionCommand
        {
            QuestionId = id,
            VoteType = request.VoteType ?? "upvote"
        });
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);
        return Ok(new { voteScore = result.Data!.VoteScore });
    }

    /// <summary>
    /// Bookmark a question
    /// </summary>
    [HttpPost("{id}/bookmark")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> BookmarkQuestion(Guid id)
    {
        // Implementation would follow similar pattern
        return Ok();
    }

    /// <summary>
    /// Get similar questions
    /// </summary>
    [HttpGet("{id}/similar")]
    [ProducesResponseType(typeof(List<QuestionDetailDto>), 200)]
    public async Task<IActionResult> GetSimilarQuestions(Guid id)
    {
        // Implementation would use AI service
        return Ok(new List<QuestionDetailDto>());
    }
}

public class QuestionListItemDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int ViewCount { get; set; }
    public int VoteScore { get; set; }
    public int AnswerCount { get; set; }
    public int CommentCount { get; set; }
    public int BookmarkCount { get; set; }
    public QuestionAuthorDto Author { get; set; } = new();
    public List<TagDto> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastActivityAt { get; set; }
}

public class QuestionAuthorDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int Reputation { get; set; }
    public string? AvatarUrl { get; set; }
}

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UsageCount { get; set; }
}

public class VoteRequest
{
    public string VoteType { get; set; } = "upvote"; // "upvote" or "downvote"
}

public class UpdateQuestionCommand
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public List<string>? Tags { get; set; }
}
