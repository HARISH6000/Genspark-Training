using InventoryManagementAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> AddCategoryAsync(AddCategoryDto categoryDto, int? currentUserId);
        Task<CategoryResponseDto?> GetCategoryByIdAsync(int categoryId);
        Task<CategoryResponseDto?> GetCategoryByNameAsync(string categoryName);
        Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<CategoryResponseDto> UpdateCategoryAsync(UpdateCategoryDto categoryDto, int? currentUserId);
        Task<CategoryResponseDto> DeleteCategoryAsync(int categoryId, int? currentUserId);
    }
}
