using MyMvcApp.Helpers;
using MyMvcApp.Models.DTOs;
using MyMvcApp.Models.Entities;
using MyMvcApp.Repository.Contracts;
using MyMvcApp.Services.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MyMvcApp.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<(int recordsTotal, int recordsFiltered, List<Category> data)> GetCategoriesForDataTableAsync(
            int start, int length, string? sortColumn, string? sortDirection, string? searchValue)
        {
            var query = _categoryRepository.GetAllData();

            // Filter
            if (!string.IsNullOrEmpty(searchValue))
            {
                var lowerSearch = searchValue.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowerSearch) ||
                                         (c.Description != null && c.Description.ToLower().Contains(lowerSearch)));
            }

            int recordsFiltered = await query.CountAsync();
            int recordsTotal = await _categoryRepository.GetAllData().CountAsync();

            // Sort
            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortDirection == "asc"
                    ? query.OrderByDynamic(sortColumn, true)
                    : query.OrderByDynamic(sortColumn, false);
            }

            // Pagination
            var data = await query.Skip(start).Take(length)
                                  .ToListAsync();

            return (recordsTotal, recordsFiltered, data);
        }

        
        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                throw new InvalidOperationException($"Category with id {id} not found.");
            return category;
        }

        public async Task<Category> CreateCategoryAsync(CreateCategoryDto dto, string webRootPath)
        {
            var category = new Category
            {
                ParentCategoryId = dto.ParentCategoryId ?? 0,
                Name = dto.Name,
                Slug = dto.Slug,
                ImageUrl = await FileHelper.SaveFileAsync(dto.ImageFile, webRootPath),
                Description = dto.Description ?? string.Empty,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            return category;
        }

        public async Task<Category> UpdateCategoryAsync(UpdateCategoryDto dto, string webRootPath)
        {
            var category = await _categoryRepository.GetByIdAsync(dto.Id);
            if (category == null)
                throw new InvalidOperationException($"Category with id {dto.Id} not found.");

            category.ParentCategoryId = dto.ParentCategoryId ?? 0;
            category.Name = dto.Name;
            category.Slug = dto.Slug;
            category.ImageUrl = await FileHelper.SaveFileAsync(dto.ImageFile, webRootPath, category.ImageUrl);
            category.Description = dto.Description ?? string.Empty;
            category.IsActive = dto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return false;

            await _categoryRepository.DeleteAsync(category);
            return true;
        }
    }
}
