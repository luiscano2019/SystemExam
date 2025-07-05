using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SystemExamApi.Models;
using SystemExamApi.Requests;
using SystemExamApi.Responses;

namespace SystemExamApi.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]

public class CategoryController : ControllerBase
{
    private readonly ExamSystemDbContext _context;

    public CategoryController(ExamSystemDbContext context)
    {
        _context = context;
    }

    // También permitir que student acceda a este método
    [Authorize(Roles = "admin,student")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryResponse>>>> GetCategories()
    {
        var categories = await _context.Categories.AsNoTracking().ToListAsync();
        var response = categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Color = c.Color,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        });
        return Ok(new ApiResponse<IEnumerable<CategoryResponse>>
        {
            Success = true,
            Message = "Categorías obtenidas correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> GetCategory(Guid id)
    {
        var c = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (c == null)
            return NotFound(new ApiResponse<CategoryResponse>
            {
                Success = false,
                Message = "Categoría no encontrada.",
                Data = null,
                Errors = new[] { "No existe una categoría con el ID proporcionado." }
            });
        var response = new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Color = c.Color,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
        return Ok(new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "Categoría obtenida correctamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryResponse>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<CategoryResponse>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var nameExists = await _context.Categories.AnyAsync(c => c.Name == request.Name);
        if (nameExists)
            return BadRequest(new ApiResponse<CategoryResponse>
            {
                Success = false,
                Message = "El nombre ya está en uso.",
                Data = null,
                Errors = new[] { "El nombre de la categoría ya existe." }
            });

        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Color = request.Color,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        var response = new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Color = category.Color,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, new ApiResponse<CategoryResponse>
        {
            Success = true,
            Message = "Categoría creada exitosamente.",
            Data = response,
            Errors = null
        });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Datos de entrada inválidos.",
                Data = null,
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            });

        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Categoría no encontrada.",
                Data = null,
                Errors = new[] { "No existe una categoría con el ID proporcionado." }
            });

        var nameExists = await _context.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id);
        if (nameExists)
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "El nombre ya está en uso.",
                Data = null,
                Errors = new[] { "El nombre de la categoría ya existe." }
            });

        category.Name = request.Name;
        category.Description = request.Description;
        category.Color = request.Color;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Categoría actualizada exitosamente.",
            Data = null,
            Errors = null
        });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCategory(Guid id)
    {
        var category = await _context.Categories.Include(c => c.Exams).FirstOrDefaultAsync(c => c.Id == id);
        if (category == null)
            return NotFound(new ApiResponse<object>
            {
                Success = false,
                Message = "Categoría no encontrada.",
                Data = null,
                Errors = new[] { "No existe una categoría con el ID proporcionado." }
            });
        if (category.Exams.Any())
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "No se puede eliminar: tiene exámenes activos.",
                Data = null,
                Errors = new[] { "La categoría tiene exámenes activos asociados." }
            });
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Categoría eliminada exitosamente.",
            Data = null,
            Errors = null
        });
    }
} 