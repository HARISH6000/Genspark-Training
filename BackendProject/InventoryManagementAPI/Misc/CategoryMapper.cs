// Mappers/CategoryMapper.cs
using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class CategoryMapper
    {
        public static Category ToCategory(AddCategoryDto dto)
        {
            return new Category
            {
                CategoryName = dto.CategoryName,
                Description = dto.Description
            };
        }

        public static void ToCategory(UpdateCategoryDto dto, Category existingCategory)
        {
            existingCategory.CategoryName = dto.CategoryName;
            existingCategory.Description = dto.Description;
        }

        public static CategoryResponseDto ToCategoryResponseDto(Category category)
        {
            return new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description
            };
        }
    }
}
