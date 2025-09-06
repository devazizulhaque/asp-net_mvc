using MyMvcApp.Models.DTOs;
using MyMvcApp.Models.Entities;

namespace MyMvcApp.Services.Contracts
{
    public interface ICategoryService
    {
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task<(int recordsTotal, int recordsFiltered, List<Category> data)> GetCategoriesForDataTableAsync(
            int start, int length, string? sortColumn, string? sortDirection, string? searchValue);
        Task<Category> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(CreateCategoryDto dto, string webRootPath);
        Task<Category> UpdateCategoryAsync(UpdateCategoryDto dto, string webRootPath);
        Task<bool> DeleteCategoryAsync(int id);
    }
}
