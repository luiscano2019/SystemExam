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
    public async Task<ActionResult<ApiResponse<ExamResponse>>> GetExam(Guid id)
    {
        var exam = await _context.Exams.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null)
            return NotFound(new ApiResponse<ExamResponse>
            {
                Success = false,
                Message = "Examen no encontrado.",
                Data = null,
                Errors = new[] { "No existe un examen con el ID proporcionado." }
            });

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

        return Ok(new ApiResponse<ExamResponse>
        {
            Success = true,
            Message = "Examen obtenido correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPost]
    //[Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<ExamResponse>>> CreateExam([FromBody] CreateExamRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<ExamResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        // Obtener el ID del usuario autenticado desde el token JWT
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized(new ApiResponse<ExamResponse>
            {
                Success = false,
                Message = "No se pudo identificar al usuario desde el token.",
                Data = null,
                Errors = new[] { "No se encontró el claim de usuario en el token." }
            });
        }

        if (!Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest(new ApiResponse<ExamResponse>
            {
                Success = false,
                Message = "El ID de usuario en el token no es válido.",
                Data = null,
                Errors = new[] { "El claim de usuario no es un GUID válido." }
            });
        }

        // Validar que la categoría exista
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new ApiResponse<ExamResponse>
            {
                Success = false,
                Message = "La categoría especificada no existe.",
                Data = null,
                Errors = new[] { "No existe una categoría con el ID proporcionado." }
            });
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

        return CreatedAtAction(nameof(GetExam), new { id = exam.Id }, new ApiResponse<ExamResponse>
        {
            Success = true,
            Message = "Examen creado exitosamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateExam(Guid id, [FromBody] UpdateExamRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Examen no encontrado.",
                Data = null,
                Errors = new[] { "No existe un examen con el ID proporcionado." }
            });

        // Validar que la categoría exista
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
        if (!categoryExists)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "La categoría especificada no existe.",
                Data = null,
                Errors = new[] { "No existe una categoría con el ID proporcionado." }
            });
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

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Examen actualizado exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteExam(Guid id)
    {
        var exam = await _context.Exams.FirstOrDefaultAsync(e => e.Id == id);
        if (exam == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Examen no encontrado.",
                Data = null,
                Errors = new[] { "No existe un examen con el ID proporcionado." }
            });

        _context.Exams.Remove(exam);
        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Examen eliminado exitosamente.",
            Data = null,
            Errors = null
        });
    }


    [HttpGet("by-category/{categoryId}")]
    public async Task<ActionResult<ApiResponse<List<ExamResponse>>>> GetExamsByCategory(Guid categoryId)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == categoryId);
        if (!categoryExists)
        {
            return NotFound(new ApiResponse<List<ExamResponse>>
            {
                Success = false,
                Message = "Categoría no encontrada.",
                Data = null,
                Errors = new[] { "No existe una categoría con el ID proporcionado." }
            });
        }

        var exams = await _context.Exams
            .AsNoTracking()
            .Where(e => e.CategoryId == categoryId)
            .ToListAsync();

        var response = exams.Select(exam => new ExamResponse
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
        }).ToList();

        return Ok(new ApiResponse<List<ExamResponse>>
        {
            Success = true,
            Message = "Exámenes obtenidos correctamente.",
            Data = response,
            Errors = null
        });
    }
}