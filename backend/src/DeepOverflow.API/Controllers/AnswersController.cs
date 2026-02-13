using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeepOverflow.Application.Common.Interfaces;
using DeepOverflow.Domain.Entities;
using DeepOverflow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DeepOverflow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnswersController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AnswersController> _logger;

    public AnswersController(
        ApplicationDbContext dbContext,
        ICurrentUserService currentUser,
        ILogger<AnswersController> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    /// <summary>
    /// Create an answer to a question
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AnswerResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateAnswer([FromBody] CreateAnswerRequest request)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Unauthorized();

        if (request.QuestionId == Guid.Empty || string.IsNullOrWhiteSpace(request.Body))
            return BadRequest(new { error = "QuestionId and body are required" });

        var question = await _dbContext.Questions
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, HttpContext.RequestAborted);

        if (question == null)
            return BadRequest(new { error = "Question not found" });

        var answer = new Answer
        {
            QuestionId = question.Id,
            Body = request.Body.Trim(),
            AuthorId = _currentUser.UserId.Value
        };

        await _dbContext.Answers.AddAsync(answer, HttpContext.RequestAborted);

        question.AnswerCount += 1;
        question.LastActivityAt = DateTime.UtcNow;
        question.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);

        return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, new AnswerResponse
        {
            Id = answer.Id,
            Body = answer.Body,
            IsAccepted = answer.IsAccepted,
            VoteScore = answer.VoteScore,
            CreatedAt = answer.CreatedAt
        });
    }

    /// <summary>
    /// Get an answer by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AnswerResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAnswer(Guid id)
    {
        var answer = await _dbContext.Answers
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, HttpContext.RequestAborted);

        if (answer == null) return NotFound();

        return Ok(new AnswerResponse
        {
            Id = answer.Id,
            Body = answer.Body,
            IsAccepted = answer.IsAccepted,
            VoteScore = answer.VoteScore,
            CreatedAt = answer.CreatedAt
        });
    }

    /// <summary>
    /// Update an answer
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateAnswer(Guid id, [FromBody] UpdateAnswerRequest request)
    {
        return Ok();
    }

    /// <summary>
    /// Delete an answer
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> DeleteAnswer(Guid id)
    {
        return NoContent();
    }

    /// <summary>
    /// Accept an answer
    /// </summary>
    [HttpPost("{id}/accept")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> AcceptAnswer(Guid id)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.UserId.HasValue)
            return Unauthorized();

        var answer = await _dbContext.Answers
            .FirstOrDefaultAsync(a => a.Id == id, HttpContext.RequestAborted);

        if (answer == null) return NotFound();

        var question = await _dbContext.Questions
            .FirstOrDefaultAsync(q => q.Id == answer.QuestionId, HttpContext.RequestAborted);

        if (question == null) return BadRequest(new { error = "Question not found" });

        var isOwner = question.AuthorId == _currentUser.UserId.Value;
        var isModerator = string.Equals(_currentUser.Role, "Moderator", StringComparison.OrdinalIgnoreCase)
            || string.Equals(_currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);

        if (!isOwner && !isModerator)
            return Forbid();

        // Unaccept previously accepted answer (if any)
        if (question.AcceptedAnswerId.HasValue && question.AcceptedAnswerId.Value != answer.Id)
        {
            var previous = await _dbContext.Answers
                .FirstOrDefaultAsync(a => a.Id == question.AcceptedAnswerId.Value, HttpContext.RequestAborted);
            previous?.Unaccept();
        }

        answer.Accept();
        question.AcceptAnswer(answer.Id);

        await _dbContext.SaveChangesAsync(HttpContext.RequestAborted);
        return Ok();
    }

    /// <summary>
    /// Vote on an answer
    /// </summary>
    [HttpPost("{id}/vote")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> VoteAnswer(Guid id, [FromBody] VoteRequest request)
    {
        return Ok(new { voteScore = 0 });
    }
}

public class CreateAnswerRequest
{
    public Guid QuestionId { get; set; }
    public string Body { get; set; } = string.Empty;
}

public class UpdateAnswerRequest
{
    public string Body { get; set; } = string.Empty;
}

public class AnswerResponse
{
    public Guid Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public bool IsAccepted { get; set; }
    public int VoteScore { get; set; }
    public DateTime CreatedAt { get; set; }
}
