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
public class ExamAnswersController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public ExamAnswersController(ExamSystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExamAnswerResponse>>> GetAnswer(Guid id)
    {
        var answer = await _context.ExamAnswers.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (answer == null)
            return NotFound(new ApiResponse<ExamAnswerResponse>
            {
                Success = false,
                Message = "Respuesta no encontrada.",
                Data = null,
                Errors = new[] { "No existe una respuesta con el ID proporcionado." }
            });

        var response = new ExamAnswerResponse
        {
            Id = answer.Id,
            AttemptId = answer.AttemptId,
            QuestionId = answer.QuestionId,
            SelectedOptions = answer.SelectedOptions,
            IsCorrect = answer.IsCorrect,
            PointsEarned = answer.PointsEarned,
            CreatedAt = answer.CreatedAt,
            UpdatedAt = answer.UpdatedAt
        };

        return Ok(new ApiResponse<ExamAnswerResponse>
        {
            Success = true,
            Message = "Respuesta obtenida correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpGet("by-attempt/{attemptId}")]
    public async Task<ActionResult<ApiResponse<List<ExamAnswerResponse>>>> GetAnswersByAttempt(Guid attemptId)
    {
        var answers = await _context.ExamAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attemptId)
            .ToListAsync();

        var response = answers.Select(answer => new ExamAnswerResponse
        {
            Id = answer.Id,
            AttemptId = answer.AttemptId,
            QuestionId = answer.QuestionId,
            SelectedOptions = answer.SelectedOptions,
            IsCorrect = answer.IsCorrect,
            PointsEarned = answer.PointsEarned,
            CreatedAt = answer.CreatedAt,
            UpdatedAt = answer.UpdatedAt
        }).ToList();

        return Ok(new ApiResponse<List<ExamAnswerResponse>>
        {
            Success = true,
            Message = "Respuestas obtenidas correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExamAnswerResponse>>> CreateAnswer([FromBody] CreateExamAnswerRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<ExamAnswerResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var answer = new ExamAnswer
        {
            Id = Guid.NewGuid(),
            AttemptId = request.AttemptId,
            QuestionId = request.QuestionId,
            SelectedOptions = request.SelectedOptions,
            IsCorrect = false, // Lógica de corrección puede ir aquí
            PointsEarned = 0,  // Lógica de puntaje puede ir aquí
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ExamAnswers.Add(answer);
        await _context.SaveChangesAsync();

        var response = new ExamAnswerResponse
        {
            Id = answer.Id,
            AttemptId = answer.AttemptId,
            QuestionId = answer.QuestionId,
            SelectedOptions = answer.SelectedOptions,
            IsCorrect = answer.IsCorrect,
            PointsEarned = answer.PointsEarned,
            CreatedAt = answer.CreatedAt,
            UpdatedAt = answer.UpdatedAt
        };

        return CreatedAtAction(nameof(GetAnswer), new { id = answer.Id }, new ApiResponse<ExamAnswerResponse>
        {
            Success = true,
            Message = "Respuesta creada exitosamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAnswer(Guid id, [FromBody] UpdateExamAnswerRequest request)
    {
        var answer = await _context.ExamAnswers.FirstOrDefaultAsync(a => a.Id == id);
        if (answer == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Respuesta no encontrada.",
                Data = null,
                Errors = new[] { "No existe una respuesta con el ID proporcionado." }
            });

        answer.SelectedOptions = request.SelectedOptions;
        if (request.IsCorrect.HasValue)
            answer.IsCorrect = request.IsCorrect.Value;
        if (request.PointsEarned.HasValue)
            answer.PointsEarned = request.PointsEarned.Value;
        answer.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Respuesta actualizada exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAnswer(Guid id)
    {
        var answer = await _context.ExamAnswers.FirstOrDefaultAsync(a => a.Id == id);
        if (answer == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Respuesta no encontrada.",
                Data = null,
                Errors = new[] { "No existe una respuesta con el ID proporcionado." }
            });

        _context.ExamAnswers.Remove(answer);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Respuesta eliminada exitosamente.",
            Data = null,
            Errors = null
        });
    }
}