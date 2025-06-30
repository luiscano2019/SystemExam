using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SystemExamApi.Models;
using SystemExamApi.Requests;
using SystemExamApi.Responses;

namespace SystemExamApi.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync();
        Task<CategoryResponse?> GetCategoryByIdAsync(Guid id);
        Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request);
        Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
        Task<bool> DeleteCategoryAsync(Guid id);
        Task<IEnumerable<object>> GetCategoryExamsAsync(Guid id);
        Task<bool> CategoryExistsAsync(Guid id);
        Task<bool> CategoryNameExistsAsync(string name, Guid? excludeId = null);
    }

    public class CategoryService : ICategoryService
    {
        private readonly ExamSystemDbContext _context;

        public CategoryService(ExamSystemDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryResponse>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Include(c => c.Exams)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ExamCount = c.Exams.Count(e => e.IsActive)
                })
                .ToListAsync();
        }

        public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id)
        {
            return await _context.Categories
                .Include(c => c.Exams)
                .Where(c => c.Id == id)
                .Select(c => new CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    Color = c.Color,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    ExamCount = c.Exams.Count(e => e.IsActive)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request)
        {
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

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                Color = category.Color,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                ExamCount = 0
            };
        }

        public async Task<bool> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return false;

            category.Name = request.Name;
            category.Description = request.Description;
            category.Color = request.Color;
            category.IsActive = request.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            var category = await _context.Categories
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            var activeExams = category.Exams.Where(e => e.IsActive).ToList();
            if (activeExams.Any())
                return false; // No se puede eliminar si tiene exámenes activos

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<object>> GetCategoryExamsAsync(Guid id)
        {
            return await _context.Exams
                .Where(e => e.CategoryId == id)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Description,
                    e.Duration,
                    e.PassingScore,
                    e.IsActive,
                    e.CreatedAt,
                    e.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<bool> CategoryExistsAsync(Guid id)
        {
            return await _context.Categories.AnyAsync(c => c.Id == id);
        }

        public async Task<bool> CategoryNameExistsAsync(string name, Guid? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _context.Categories.AnyAsync(c => c.Name == name && c.Id != excludeId.Value);
            
            return await _context.Categories.AnyAsync(c => c.Name == name);
        }
    }
} 