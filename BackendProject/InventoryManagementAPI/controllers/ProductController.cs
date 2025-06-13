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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProduct([FromBody] AddProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId();
                var newProduct = await _productService.AddProductAsync(productDto, currentUserId); // Pass currentUserId
                return CreatedAtAction(nameof(GetProductById), new { productId = newProduct.ProductId }, newProduct);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Product creation conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (NotFoundException ex) // Added for category not found
            {
                _logger.LogWarning(ex, "Product creation failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during product creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during product creation." });
            }
        }

        
        [HttpGet("{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProductById(int productId)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound(new { message = $"Product with ID {productId} not found." });
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving product by ID {ProductId}.", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllProducts([FromQuery] bool includeDeleted = false)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync(includeDeleted);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all products.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        
        [HttpPut]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                var updatedProduct = await _productService.UpdateProductAsync(productDto, currentUserId); // Pass currentUserId
                return Ok(updatedProduct);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Product update failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Product update conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during product update.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during product update." });
            }
        }

        
        [HttpDelete("softdelete/{productId}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeleteProduct(int productId)
        {
            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                var product = await _productService.SoftDeleteProductAsync(productId, currentUserId); // Pass currentUserId
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Product soft delete failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during product soft delete for ID {ProductId}.", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during soft deletion." });
            }
        }

        
        [HttpDelete("harddelete/{productId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HardDeleteProduct(int productId)
        {
            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID
                _logger.LogInformation("Attempting to hard delete product with ID {ProductId} by user {UserId}.", productId, currentUserId);
                var product = await _productService.HardDeleteProductAsync(productId, currentUserId); // Pass currentUserId
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Product hard delete failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during product hard delete for ID {ProductId}.", productId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during hard deletion." });
            }
        }
    }
}
