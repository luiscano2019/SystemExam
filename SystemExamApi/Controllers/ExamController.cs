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
public class ExamController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public ExamController(ExamSystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExamResponse>> GetExam(Guid id)
    {
        var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null)
            return NotFound(new { message = "Examen no encontrado" });

        var response = new ExamResponse
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            CategoryId = exam.CategoryId,
            Duration = exam.Duration,
            TotalQuestions = exam.TotalQuestions,
            PassingScore = exam.PassingScore,
            IsActive = exam.IsActive,
            RandomizeQuestions = exam.RandomizeQuestions,
            CreatedBy = exam.CreatedBy,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ExamResponse>> CreateExam([FromBody] CreateExamRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Obtener el ID del usuario autenticado desde el token JWT
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "No se pudo identificar al usuario desde el token." });
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest(new { message = "El ID de usuario en el token no es válido." });
        }

        // Validar que la categoría exista
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "La categoría especificada no existe." });
        }

        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Duration = request.Duration,
            TotalQuestions = request.TotalQuestions,
            PassingScore = request.PassingScore,
            IsActive = request.IsActive,
            RandomizeQuestions = request.RandomizeQuestions,
            CreatedBy = userId, // Usar el ID del usuario autenticado
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Exams.Add(exam);
        await _context.SaveChangesAsync();

        var response = new ExamResponse
        {
            Id = exam.Id,
            Title = exam.Title,
            Description = exam.Description,
            CategoryId = exam.CategoryId,
            Duration = exam.Duration,
            TotalQuestions = exam.TotalQuestions,
            PassingScore = exam.PassingScore,
            IsActive = exam.IsActive,
            RandomizeQuestions = exam.RandomizeQuestions,
            CreatedBy = exam.CreatedBy,
            CreatedAt = exam.CreatedAt,
            UpdatedAt = exam.UpdatedAt
        };

        return CreatedAtAction(nameof(GetExam), new { id = exam.Id }, response);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateExam(Guid id, [FromBody] UpdateExamRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null)
            return NotFound(new { message = "Examen no encontrado" });

        // Validar que la categoría exista
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new { message = "La categoría especificada no existe." });
        }


        exam.Title = request.Title;
        exam.Description = request.Description;
        exam.CategoryId = request.CategoryId;
        exam.Duration = request.Duration;
        exam.TotalQuestions = request.TotalQuestions;
        exam.PassingScore = request.PassingScore;
        exam.IsActive = request.IsActive;
        exam.RandomizeQuestions = request.RandomizeQuestions;
        exam.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteExam(Guid id)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null)
            return NotFound(new { message = "Examen no encontrado" });

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}