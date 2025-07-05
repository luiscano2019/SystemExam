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
public class ExamAttemptsController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public ExamAttemptsController(ExamSystemDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ExamAttemptResponse>>> GetAttempt(Guid id)
    {
        var attempt = await _context.ExamAttempts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (attempt == null)
            return NotFound(new ApiResponse<ExamAttemptResponse>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });

        var response = new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        };

        return Ok(new ApiResponse<ExamAttemptResponse>
        {
            Success = true,
            Message = "Intento obtenido correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpGet("by-exam/{examId}")]
    public async Task<ActionResult<ApiResponse<List<ExamAttemptResponse>>>> GetAttemptsByExam(Guid examId)
    {
        var attempts = await _context.ExamAttempts
            .AsNoTracking()
            .Where(a => a.ExamId == examId)
            .ToListAsync();

        var response = attempts.Select(attempt => new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        }).ToList();

        return Ok(new ApiResponse<List<ExamAttemptResponse>>
        {
            Success = true,
            Message = "Intentos obtenidos correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExamAttemptResponse>>> CreateAttempt([FromBody] CreateExamAttemptRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<ExamAttemptResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var attempt = new ExamAttempt
        {
            Id = Guid.NewGuid(),
            ExamId = request.ExamId,
            StudentId = request.StudentId,
            StartedAt = DateTime.UtcNow,
            Status = "started",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ExamAttempts.Add(attempt);
        await _context.SaveChangesAsync();

        var response = new ExamAttemptResponse
        {
            Id = attempt.Id,
            ExamId = attempt.ExamId,
            StudentId = attempt.StudentId,
            StartedAt = attempt.StartedAt,
            CompletedAt = attempt.CompletedAt,
            Score = attempt.Score,
            TotalPoints = attempt.TotalPoints,
            EarnedPoints = attempt.EarnedPoints,
            Status = attempt.Status,
            TimeSpent = attempt.TimeSpent,
            IpAddress = attempt.IpAddress,
            UserAgent = attempt.UserAgent,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt
        };

        return CreatedAtAction(nameof(GetAttempt), new { id = attempt.Id }, new ApiResponse<ExamAttemptResponse>
        {
            Success = true,
            Message = "Intento creado exitosamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateAttempt(Guid id, [FromBody] UpdateExamAttemptRequest request)
    {
        var attempt = await _context.ExamAttempts.FirstOrDefaultAsync(a => a.Id == id);
        if (attempt == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });

        attempt.CompletedAt = request.CompletedAt ?? attempt.CompletedAt;
        attempt.Score = request.Score ?? attempt.Score;
        attempt.TotalPoints = request.TotalPoints ?? attempt.TotalPoints;
        attempt.EarnedPoints = request.EarnedPoints ?? attempt.EarnedPoints;
        attempt.Status = request.Status ?? attempt.Status;
        attempt.TimeSpent = request.TimeSpent ?? attempt.TimeSpent;
        attempt.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Intento actualizado exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAttempt(Guid id)
    {
        var attempt = await _context.ExamAttempts.FirstOrDefaultAsync(a => a.Id == id);
        if (attempt == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Intento no encontrado.",
                Data = null,
                Errors = new[] { "No existe un intento con el ID proporcionado." }
            });

        _context.ExamAttempts.Remove(attempt);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Intento eliminado exitosamente.",
            Data = null,
            Errors = null
        });
    }
}