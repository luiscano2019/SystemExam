using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemExamApi.Models;
using SystemExamApi.Requests;
using SystemExamApi.Responses;
using System.Security.Claims;

namespace SystemExamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public QuestionController(ExamSystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionResponse>> GetQuestion(Guid id)
    {
        var question = await _context.Questions
            .Include(q => q.QuestionOptions)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound(new { message = "Pregunta no encontrada" });

        var response = new QuestionResponse
        {
            Id = question.Id,
            ExamId = question.ExamId,
            Text = question.Text,
            Type = question.Type,
            Points = question.Points,
            OrderNumber = question.OrderNumber,
            Explanation = question.Explanation,
            IsActive = question.IsActive,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Options = question.QuestionOptions.Select(opt => new QuestionOptionResponse
            {
                Id = opt.Id,
                Text = opt.Text,
                IsCorrect = opt.IsCorrect,
                OrderNumber = opt.OrderNumber,
                CreatedAt = opt.CreatedAt,
                UpdatedAt = opt.UpdatedAt
            }).ToList()
        };
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<QuestionResponse>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var examExists = await _context.Exams.AnyAsync(e => e.Id == request.ExamId);
        if (!examExists)
            return BadRequest(new { message = "El examen especificado no existe." });

        var question = new Question
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            Text = request.Text,
            Type = request.Type,
            Points = request.Points,
            OrderNumber = request.OrderNumber,
            Explanation = request.Explanation,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.Options != null && request.Options.Any())
        {
            foreach (var opt in request.Options)
            {
                question.QuestionOptions.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Text = opt.Text,
                    IsCorrect = opt.IsCorrect,
                    OrderNumber = opt.OrderNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        _context.Questions.Add(question);
        await _context.SaveChangesAsync();

        var response = new QuestionResponse
        {
            Id = question.Id,
            ExamId = question.ExamId,
            Text = question.Text,
            Type = question.Type,
            Points = question.Points,
            OrderNumber = question.OrderNumber,
            Explanation = question.Explanation,
            IsActive = question.IsActive,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Options = question.QuestionOptions.Select(opt => new QuestionOptionResponse
            {
                Id = opt.Id,
                Text = opt.Text,
                IsCorrect = opt.IsCorrect,
                OrderNumber = opt.OrderNumber,
                CreatedAt = opt.CreatedAt,
                UpdatedAt = opt.UpdatedAt
            }).ToList()
        };
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var question = await _context.Questions.Include(q => q.QuestionOptions).FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound(new { message = "Pregunta no encontrada" });

        question.Text = request.Text;
        question.Type = request.Type;
        question.Points = request.Points;
        question.OrderNumber = request.OrderNumber;
        question.Explanation = request.Explanation;
        question.IsActive = request.IsActive;
        question.UpdatedAt = DateTime.UtcNow;

        // Opciones (actualización simple: eliminar y volver a agregar)
        if (request.Options != null)
        {
            _context.QuestionOptions.RemoveRange(question.QuestionOptions);
            foreach (var opt in request.Options)
            {
                question.QuestionOptions.Add(new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    Text = opt.Text,
                    IsCorrect = opt.IsCorrect,
                    OrderNumber = opt.OrderNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteQuestion(Guid id)
    {
        var question = await _context.Questions.Include(q => q.QuestionOptions).FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound(new { message = "Pregunta no encontrada" });

        _context.QuestionOptions.RemoveRange(question.QuestionOptions);
        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        return NoContent();
    }
} 