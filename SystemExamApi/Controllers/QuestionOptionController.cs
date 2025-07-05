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
    public async Task<ActionResult<ApiResponse<QuestionOptionResponse>>> GetOption(Guid id)
    {
        var option = await _context.QuestionOptions.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id);
        if (option == null)
            return NotFound(new ApiResponse<QuestionOptionResponse>
            {
                Success = false,
                Message = "Opción no encontrada.",
                Data = null,
                Errors = new[] { "No existe una opción con el ID proporcionado." }
            });

        var response = new QuestionOptionResponse
        {
            Id = option.Id,
            Text = option.Text,
            IsCorrect = option.IsCorrect,
            OrderNumber = option.OrderNumber,
            CreatedAt = option.CreatedAt,
            UpdatedAt = option.UpdatedAt
        };
        return Ok(new ApiResponse<QuestionOptionResponse>
        {
            Success = true,
            Message = "Opción obtenida correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<QuestionOptionResponse>>> CreateOption([FromBody] CreateQuestionOptionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<QuestionOptionResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId);
        if (question == null)
            return BadRequest(new ApiResponse<QuestionOptionResponse>
            {
                Success = false,
                Message = "La pregunta especificada no existe.",
                Data = null,
                Errors = new[] { "No existe una pregunta con el ID proporcionado." }
            });

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
        return CreatedAtAction(nameof(GetOption), new { id = option.Id }, new ApiResponse<QuestionOptionResponse>
        {
            Success = true,
            Message = "Opción creada exitosamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateOption(Guid id, [FromBody] UpdateQuestionOptionRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var option = await _context.QuestionOptions.FirstOrDefaultAsync(o => o.Id == id);
        if (option == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Opción no encontrada.",
                Data = null,
                Errors = new[] { "No existe una opción con el ID proporcionado." }
            });

        var question = await _context.Questions.FirstOrDefaultAsync(q => q.Id == request.QuestionId);
        if (question == null)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "La pregunta especificada no existe.",
                Data = null,
                Errors = new[] { "No existe una pregunta con el ID proporcionado." }
            });

        option.QuestionId = request.QuestionId;
        option.Text = request.Text;
        option.IsCorrect = request.IsCorrect;
        option.OrderNumber = request.OrderNumber;
        option.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Opción actualizada exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteOption(Guid id)
    {
        var option = await _context.QuestionOptions.FirstOrDefaultAsync(o => o.Id == id);
        if (option == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Opción no encontrada.",
                Data = null,
                Errors = new[] { "No existe una opción con el ID proporcionado." }
            });

        _context.QuestionOptions.Remove(option);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Opción eliminada exitosamente.",
            Data = null,
            Errors = null
        });
    }
} 