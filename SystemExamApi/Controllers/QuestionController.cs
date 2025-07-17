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
    public async Task<ActionResult<ApiResponse<QuestionResponse>>> GetQuestion(Guid id)
    {
        var question = await _context.Questions
            .Include(q => q.QuestionOptions)
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound(new ApiResponse<QuestionResponse>
            {
                Success = false,
                Message = "Pregunta no encontrada.",
                Data = null,
                Errors = new[] { "No existe una pregunta con el ID proporcionado." }
            });

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
        return Ok(new ApiResponse<QuestionResponse>
        {
            Success = true,
            Message = "Pregunta obtenida correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<QuestionResponse>>> CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<QuestionResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var examExists = await _context.Exams.AnyAsync(e => e.Id == request.ExamId);
        if (!examExists)
            return BadRequest(new ApiResponse<QuestionResponse>
            {
                Success = false,
                Message = "El examen especificado no existe.",
                Data = null,
                Errors = new[] { "No existe un examen con el ID proporcionado." }
            });

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
        return CreatedAtAction(nameof(GetQuestion), new { id = question.Id }, new ApiResponse<QuestionResponse>
        {
            Success = true,
            Message = "Pregunta creada exitosamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateQuestion(Guid id, [FromBody] UpdateQuestionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var question = await _context.Questions.Include(q => q.QuestionOptions).FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Pregunta no encontrada.",
                Data = null,
                Errors = new[] { "No existe una pregunta con el ID proporcionado." }
            });

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
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Pregunta actualizada exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteQuestion(Guid id)
    {
        var question = await _context.Questions.Include(q => q.QuestionOptions).FirstOrDefaultAsync(q => q.Id == id);
        if (question == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Pregunta no encontrada.",
                Data = null,
                Errors = new[] { "No existe una pregunta con el ID proporcionado." }
            });

        _context.QuestionOptions.RemoveRange(question.QuestionOptions);
        _context.Questions.Remove(question);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Pregunta eliminada exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpGet("by-exam/{examId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<QuestionResponse>>>> GetQuestionsByExam(Guid examId)
    {
        var questions = await _context.Questions
            .Where(q => q.ExamId == examId)
            .Include(q => q.QuestionOptions)
            .OrderBy(q => q.OrderNumber)
            .AsNoTracking()
            .ToListAsync();

        if (!questions.Any())
        {
            return NotFound(new ApiResponse<IEnumerable<QuestionResponse>>
            {
                Success = false,
                Message = "No se encontraron preguntas para el examen especificado.",
                Data = null,
                Errors = new[] { "No existen preguntas para el Id de examen proporcionado." }
            });
        }

        var response = questions.Select(question => new QuestionResponse
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
        });

        return Ok(new ApiResponse<IEnumerable<QuestionResponse>>
        {
            Success = true,
            Message = "Preguntas obtenidas correctamente.",
            Data = response,
            Errors = null
        });
    }
} 