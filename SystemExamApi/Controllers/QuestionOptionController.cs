using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemExamApi.Models;
using SystemExamApi.Requests;
using SystemExamApi.Responses;

namespace SystemExamApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuestionOptionController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public QuestionOptionController(ExamSystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<QuestionOptionResponse>> GetOption(Guid id)
    {
        var option = await _context.QuestionOptions.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (option == null)
            return NotFound(new { message = "Opción no encontrada" });

        var response = new QuestionOptionResponse
        {
            Id = option.Id,
            Text = option.Text,
            IsCorrect = option.IsCorrect,
            OrderNumber = option.OrderNumber,
            CreatedAt = option.CreatedAt,
            UpdatedAt = option.UpdatedAt
        };
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<QuestionOptionResponse>> CreateOption([FromBody] CreateQuestionOptionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId);
        if (question == null)
            return BadRequest(new { message = "La pregunta especificada no existe." });

        var option = new QuestionOption
        {
            Id = Guid.NewGuid(),
            QuestionId = request.QuestionId,
            Text = request.Text,
            IsCorrect = request.IsCorrect,
            OrderNumber = request.OrderNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.QuestionOptions.Add(option);
        await _context.SaveChangesAsync();

        var response = new QuestionOptionResponse
        {
            Id = option.Id,
            Text = option.Text,
            IsCorrect = option.IsCorrect,
            OrderNumber = option.OrderNumber,
            CreatedAt = option.CreatedAt,
            UpdatedAt = option.UpdatedAt
        };
        return CreatedAtAction(nameof(GetOption), new { id = option.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateOption(Guid id, [FromBody] UpdateQuestionOptionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var option = await _context.QuestionOptions.FirstOrDefaultAsync(o => o.Id == id);
        if (option == null)
            return NotFound(new { message = "Opción no encontrada" });

        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId);
        if (question == null)
            return BadRequest(new { message = "La pregunta especificada no existe." });

        option.QuestionId = request.QuestionId;
        option.Text = request.Text;
        option.IsCorrect = request.IsCorrect;
        option.OrderNumber = request.OrderNumber;
        option.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteOption(Guid id)
    {
        var option = await _context.QuestionOptions.FirstOrDefaultAsync(o => o.Id == id);
        if (option == null)
            return NotFound(new { message = "Opción no encontrada" });

        _context.QuestionOptions.Remove(option);
        await _context.SaveChangesAsync();
        return NoContent();
    }
} 