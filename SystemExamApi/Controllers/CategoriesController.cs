using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SystemExamApi.Services;
using SystemExamApi.Responses;
using SystemExamApi.Requests;

namespace SystemExamApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener categorías", error = ex.Message });
            }
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryResponse>> GetCategory(Guid id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category is null) // Use 'is null' instead of '== null' for nullable types
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(category);
        }

        // POST: api/Categories
        [HttpPost]
        public async Task<ActionResult<CategoryResponse>> PostCategory([FromBody] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nameExists = await _categoryService.CategoryNameExistsAsync(request.Name);
            if (nameExists)
                return BadRequest(new { message = "El nombre ya está en uso" });

            try
            {
                var category = await _categoryService.CreateCategoryAsync(request);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la categoría", error = ex.Message });
            }
        }

        // PUT: api/Categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(Guid id, UpdateCategoryRequest request)
        {
            if (id == Guid.Empty)
                return BadRequest("ID inválido");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var nameExists = await _categoryService.CategoryNameExistsAsync(request.Name, id);
            if (nameExists)
                return BadRequest(new { message = "El nombre ya está en uso" });

            var updated = await _categoryService.UpdateCategoryAsync(id, request);
            if (!updated)
                return NotFound(new { message = "Categoría no encontrada" });

            return Ok(new { message = "Categoría actualizada exitosamente" });
        }

        // DELETE: api/Categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var deleted = await _categoryService.DeleteCategoryAsync(id);
            if (!deleted)
            {
                var categoryExists = await _categoryService.CategoryExistsAsync(id);
                if (!categoryExists)
                    return NotFound(new { message = "Categoría no encontrada" });
                
                return BadRequest(new { message = "No se puede eliminar: tiene exámenes activos" });
            }

            return Ok(new { message = "Categoría eliminada exitosamente" });
        }

        // GET: api/Categories/5/exams
        [HttpGet("{id}/exams")]
        public async Task<ActionResult<IEnumerable<object>>> GetCategoryExams(Guid id)
        {
            var categoryExists = await _categoryService.CategoryExistsAsync(id);
            if (!categoryExists)
                return NotFound(new { message = "Categoría no encontrada" });

            var exams = await _categoryService.GetCategoryExamsAsync(id);
            return Ok(exams);
        }
    }
} 