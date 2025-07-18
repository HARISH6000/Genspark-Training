using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Utilities;

namespace InventoryManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryProductsController : ControllerBase
    {
        private readonly IInventoryProductService _inventoryProductService;
        private readonly ILogger<InventoryProductsController> _logger;

        public InventoryProductsController(IInventoryProductService inventoryProductService, ILogger<InventoryProductsController> logger)
        {
            _inventoryProductService = inventoryProductService;
            _logger = logger;
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddInventoryProduct([FromBody] AddInventoryProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                var newEntry = await _inventoryProductService.AddInventoryProductAsync(dto, currentUserId); // Pass currentUserId
                return StatusCode(StatusCodes.Status201Created, newEntry);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation forbidden: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation failed due to invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during inventory product creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during inventory product creation." });
            }
        }


        [HttpPut("increase-quantity")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> IncreaseProductQuantity([FromBody] AdjustProductQuantityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = User.GetUserId();
                var updatedEntry = await _inventoryProductService.IncreaseProductQuantityAsync(dto, currentUserId); // Pass currentUserId
                return Ok(updatedEntry);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to increase product quantity: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to increase product quantity due to invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation forbidden: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while increasing product quantity.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpPut("decrease-quantity")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DecreaseProductQuantity([FromBody] AdjustProductQuantityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                var updatedEntry = await _inventoryProductService.DecreaseProductQuantityAsync(dto, currentUserId);
                return Ok(updatedEntry);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to decrease product quantity: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to decrease product quantity due to invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation forbidden: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Failed to decrease product quantity due to invalid operation: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while decreasing product quantity.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpPut("set-quantity")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SetProductQuantity([FromBody] SetProductQuantityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                var updatedEntry = await _inventoryProductService.SetProductQuantityAsync(dto, currentUserId); // Pass currentUserId
                return Ok(updatedEntry);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to set product quantity: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Failed to set product quantity due to invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting product quantity.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPut("update-minstock")] // <--- ADDED New Endpoint
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateMinStockQuantity([FromBody] UpdateInventoryProductMinStockDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = User.GetUserId();
                var updatedEntry = await _inventoryProductService.UpdateMinStockQuantityAsync(dto, currentUserId); // <--- Call new service method
                return Ok(updatedEntry);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to update minimum stock quantity: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation forbidden: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (ArgumentException ex) // For example, if NewMinStockQuantity was negative (though DTO validation should catch this)
            {
                _logger.LogWarning(ex, "Failed to update minimum stock quantity due to invalid argument: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating minimum stock quantity.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpDelete("{inventoryProductId}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteInventoryProduct(int inventoryProductId)
        {
            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                var deletedEntry = await _inventoryProductService.DeleteInventoryProductAsync(inventoryProductId, currentUserId); // Pass currentUserId
                return Ok(deletedEntry);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to delete inventory product: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ForbiddenException ex)
            {
                _logger.LogWarning(ex, "Inventory product creation forbidden: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during inventory product deletion for ID {InventoryProductId}.", inventoryProductId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("{inventoryProductId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryProductById(int inventoryProductId)
        {
            try
            {
                var entry = await _inventoryProductService.GetInventoryProductByIdAsync(inventoryProductId);
                if (entry == null)
                {
                    return NotFound(new { message = $"InventoryProduct entry with ID {inventoryProductId} not found." });
                }
                return Ok(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving inventory product by ID {InventoryProductId}.", inventoryProductId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("by-inventory/{inventoryId}/product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryProductByInventoryAndProductId(int inventoryId, int productId)
        {
            try
            {
                var entry = await _inventoryProductService.GetInventoryProductByInventoryAndProductIdAsync(inventoryId, productId);
                if (entry == null)
                {
                    return NotFound(new { message = $"Product ID {productId} not found in Inventory ID {inventoryId}." });
                }
                return Ok(entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving inventory product by Inventory ID {InventoryId} and Product ID {ProductId}.", inventoryId, productId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryProductResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInventoryProducts([FromQuery] string? sortBy = null)
        {
            try
            {
                var entries = await _inventoryProductService.GetAllInventoryProductsAsync(sortBy);
                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all inventory products.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("products-in-inventory/{inventoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResponse<ProductInInventoryResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsInInventory(int inventoryId,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? orderBy = null)
        {
            try
            {
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    var products = await _inventoryProductService.GetProductsInInventoryAsync(inventoryId, pageNumber.Value, pageSize.Value, searchTerm, orderBy);
                    return Ok(products);
                }
                else
                {
                    var products = await _inventoryProductService.GetProductsInInventoryAsync(inventoryId, orderBy);
                    return Ok(new PaginationResponse<ProductInInventoryResponseDto> { Data = products, Pagination = null });
                }
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve products for inventory: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products for inventory ID {InventoryId}.", inventoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("products-in-inventory/{inventoryId}/by-category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductInInventoryResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductsInInventoryByCategory(int inventoryId, int categoryId, [FromQuery] string? sortBy = null)
        {
            try
            {
                var products = await _inventoryProductService.GetProductsInInventoryByCategoryAsync(inventoryId, categoryId, sortBy);
                if (!products.Any())
                {
                    return Ok(products);
                }
                return Ok(products);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve products for inventory by category: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving products for inventory ID {InventoryId} and category ID {CategoryId}.", inventoryId, categoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("inventories-for-product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResponse<InventoryForProductResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoriesForProduct(int productId,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? orderBy = null)
        {
            try
            {
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    var inventories = await _inventoryProductService.GetInventoriesForProductAsync(productId, pageNumber.Value, pageSize.Value, searchTerm, orderBy);
                    return Ok(inventories);
                }
                else
                {
                    var inventories = await _inventoryProductService.GetInventoriesForProductAsync(productId, orderBy);
                    return Ok(new PaginationResponse<InventoryForProductResponseDto> { Data = inventories, Pagination = null });
                }
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve inventories for product: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving inventories for product ID {ProductId}.", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("inventories-for-product-by-sku/{sku}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryForProductResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoriesForProductBySKU(string sku, [FromQuery] string? sortBy = null)
        {
            try
            {
                var inventories = await _inventoryProductService.GetInventoriesForProductBySKUAsync(sku, sortBy);
                if (!inventories.Any())
                {
                    return Ok(inventories);
                }
                return Ok(inventories);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve inventories for product by SKU: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving inventories for product SKU {SKU}.", sku);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("low-stock/{inventoryId}/{threshold}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductInInventoryResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLowStockProductsInInventory(int inventoryId, int threshold, [FromQuery] string? sortBy = null)
        {
            try
            {
                var products = await _inventoryProductService.GetLowStockProductsInInventoryAsync(inventoryId, threshold, sortBy);
                if (!products.Any())
                {
                    return Ok(products);
                }
                return Ok(products);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve low stock products: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving low stock products for inventory ID {InventoryId} with threshold {Threshold}.", inventoryId, threshold);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}
