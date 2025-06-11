using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Utilities;

namespace InventoryManagementAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize] // All actions require authentication
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Adds a new product category.
        /// </summary>
        /// <param name="categoryDto">The category data to add.</param>
        /// <returns>The newly created category.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can add categories
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId();
                var newCategory = await _categoryService.AddCategoryAsync(categoryDto, currentUserId);
                return CreatedAtAction(nameof(GetCategoryById), new { categoryId = newCategory.CategoryId }, newCategory);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Category creation conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during category creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during category creation." });
            }
        }

        /// <summary>
        /// Retrieves a category by its ID.
        /// </summary>
        /// <param name="categoryId">The ID of the category.</param>
        /// <returns>The category details.</returns>
        [HttpGet("{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                if (category == null)
                {
                    return NotFound(new { message = $"Category with ID {categoryId} not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving category by ID {CategoryId}.", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
        
        /// <summary>
        /// Retrieves a category by its name.
        /// </summary>
        /// <param name="categoryName">The name of the category.</param>
        /// <returns>The category details.</returns>
        [HttpGet("by-name/{categoryName}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryByName(string categoryName)
        {
            try
            {
                var category = await _categoryService.GetCategoryByNameAsync(categoryName);
                if (category == null)
                {
                    return NotFound(new { message = $"Category with name '{categoryName}' not found." });
                }
                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving category by name {CategoryName}.", categoryName);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        /// <summary>
        /// Retrieves all product categories.
        /// </summary>
        /// <returns>A list of all categories.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoryResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all categories.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Updates an existing product category.
        /// </summary>
        /// <param name="categoryDto">The updated category data.</param>
        /// <returns>The updated category.</returns>
        [HttpPut]
        [Authorize(Roles = "Admin")] // Only Admin can update categories
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId();
                var updatedCategory = await _categoryService.UpdateCategoryAsync(categoryDto, currentUserId);
                return Ok(updatedCategory);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Category update failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Category update conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during category update.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during category update." });
            }
        }

        /// <summary>
        /// Deletes a product category by its ID.
        /// Note: This will likely fail if products are still associated with this category due to foreign key constraints.
        /// </summary>
        /// <param name="categoryId">The ID of the category to delete.</param>
        /// <returns>The deleted category.</returns>
        [HttpDelete("{categoryId}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete categories
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)] // For FK violation
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            try
            {
                var currentUserId = User.GetUserId();
                var deletedCategory = await _categoryService.DeleteCategoryAsync(categoryId, currentUserId);
                return Ok(deletedCategory);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Category deletion failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (DbUpdateException ex)
            {
                 // Check for foreign key violation message specifically
                if (ex.InnerException?.Message.Contains("foreign key constraint fails") == true || ex.InnerException?.Message.Contains("violates foreign key constraint") == true)
                {
                    _logger.LogWarning(ex, "Category deletion failed due to associated products: {Message}", ex.Message);
                    return Conflict(new { message = "Cannot delete category because it is associated with existing products. Please remove or reassign products first." });
                }
                _logger.LogError(ex, "An error occurred during category deletion for ID {CategoryId}.", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during category deletion." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during category deletion for ID {CategoryId}.", categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during category deletion." });
            }
        }
    }
}
