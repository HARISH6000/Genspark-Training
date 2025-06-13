using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using Microsoft.EntityFrameworkCore; 
using System.Text.Json;

namespace InventoryManagementAPI.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditLogService _auditLogService;

        public CategoryService(ICategoryRepository categoryRepository, IAuditLogService auditLogService)
        {
            _categoryRepository = categoryRepository;
            _auditLogService = auditLogService;
        }

        public async Task<CategoryResponseDto> AddCategoryAsync(AddCategoryDto categoryDto, int? currentUserId)
        {
            var existingCategory = await _categoryRepository.GetByName(categoryDto.CategoryName);
            if (existingCategory != null)
            {
                throw new ConflictException($"Category with name '{categoryDto.CategoryName}' already exists.");
            }

            var newCategory = CategoryMapper.ToCategory(categoryDto);

            try
            {
                var addedCategory = await _categoryRepository.Add(newCategory);
                
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Categories",
                    RecordId = addedCategory.CategoryId.ToString(),
                    ActionType = "INSERT",
                    NewValues = addedCategory
                });

                return CategoryMapper.ToCategoryResponseDto(addedCategory);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("Name") || ex.InnerException.Message.Contains("name")) 
                        throw new ConflictException($"Category with name '{categoryDto.CategoryName}' already exists. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.Get(categoryId);
            if (category == null) return null;
            return CategoryMapper.ToCategoryResponseDto(category);
        }
        
        public async Task<CategoryResponseDto?> GetCategoryByNameAsync(string categoryName)
        {
            var category = await _categoryRepository.GetByName(categoryName);
            if (category == null) return null;
            return CategoryMapper.ToCategoryResponseDto(category);
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAll();
            return categories.Select(c => CategoryMapper.ToCategoryResponseDto(c));
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(UpdateCategoryDto categoryDto, int? currentUserId)
        {
            var existingCategory = await _categoryRepository.Get(categoryDto.CategoryId);
            if (existingCategory == null)
            {
                throw new NotFoundException($"Category with ID {categoryDto.CategoryId} not found.");
            }

            // Capture old state before modifications
            var oldCategorySnapshot = JsonSerializer.Deserialize<Category>(JsonSerializer.Serialize(existingCategory));

            // Check for name uniqueness if name is being changed
            if (existingCategory.CategoryName != categoryDto.CategoryName)
            {
                var categoryWithSameName = await _categoryRepository.GetByName(categoryDto.CategoryName);
                if (categoryWithSameName != null && categoryWithSameName.CategoryId != existingCategory.CategoryId)
                {
                    throw new ConflictException($"Category with name '{categoryDto.CategoryName}' already exists.");
                }
            }

            CategoryMapper.ToCategory(categoryDto, existingCategory);

            try
            {
                var updatedCategory = await _categoryRepository.Update(categoryDto.CategoryId, existingCategory);
                
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Categories",
                    RecordId = updatedCategory.CategoryId.ToString(),
                    ActionType = "UPDATE",
                    OldValues = oldCategorySnapshot,
                    NewValues = updatedCategory
                });

                return CategoryMapper.ToCategoryResponseDto(updatedCategory);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("Name") || ex.InnerException.Message.Contains("name"))
                        throw new ConflictException($"Category with name '{categoryDto.CategoryName}' already exists. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<CategoryResponseDto> DeleteCategoryAsync(int categoryId, int? currentUserId)
        {
            var categoryToDelete = await _categoryRepository.Get(categoryId);
            if (categoryToDelete == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            }
            

            var oldCategorySnapshot = JsonSerializer.Deserialize<Category>(JsonSerializer.Serialize(categoryToDelete));

            var deletedCategory = await _categoryRepository.Delete(categoryId);
            
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Categories",
                RecordId = deletedCategory.CategoryId.ToString(),
                ActionType = "DELETE",
                OldValues = oldCategorySnapshot,
                NewValues = null 
            });

            return CategoryMapper.ToCategoryResponseDto(deletedCategory);
        }
    }
}
