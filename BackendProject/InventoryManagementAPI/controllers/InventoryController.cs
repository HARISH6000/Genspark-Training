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
    public class InventoriesController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoriesController> _logger;

        public InventoriesController(IInventoryService inventoryService, ILogger<InventoriesController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(InventoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddInventory([FromBody] AddInventoryDto inventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId(); // Get current user ID from extension method
                var newInventory = await _inventoryService.AddInventoryAsync(inventoryDto, currentUserId); 
                return CreatedAtAction(nameof(GetInventoryById), new { inventoryId = newInventory.InventoryId }, newInventory);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Inventory creation conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during inventory creation.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during inventory creation." });
            }
        }

        
        [HttpGet("{inventoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryById(int inventoryId)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
                if (inventory == null)
                {
                    return NotFound(new { message = $"Inventory with ID {inventoryId} not found." });
                }
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving inventory by ID {InventoryId}.", inventoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResponse<InventoryResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInventories(
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] bool includeDeleted = false,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? orderBy = null)
        {
            try
            {
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    var inventories = await _inventoryService.GetAllInventoriesAsync(pageNumber.Value, pageSize.Value, searchTerm, orderBy, includeDeleted);
                    return Ok(inventories);
                }
                else
                {
                    var inventories = await _inventoryService.GetAllInventoriesAsync(includeDeleted);
                    return Ok(new PaginationResponse<InventoryResponseDto> { Data=inventories, Pagination=null });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all inventories.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        
        [HttpPut]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateInventory([FromBody] UpdateInventoryDto inventoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId(); 
                var updatedInventory = await _inventoryService.UpdateInventoryAsync(inventoryDto, currentUserId);
                return Ok(updatedInventory);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Inventory update failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                    _logger.LogWarning(ex, "Inventory update conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during inventory update.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during inventory update." });
            }
        }

        
        [HttpDelete("softdelete/{inventoryId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SoftDeleteInventory(int inventoryId)
        {
            try
            {
                var currentUserId = User.GetUserId(); 
                var inventory = await _inventoryService.SoftDeleteInventoryAsync(inventoryId, currentUserId); 
                return Ok(inventory);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Inventory soft delete failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during inventory soft delete for ID {InventoryId}.", inventoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during soft deletion." });
            }
        }

        
        [HttpDelete("harddelete/{inventoryId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HardDeleteInventory(int inventoryId)
        {
            try
            {
                var currentUserId = User.GetUserId();
                var inventory = await _inventoryService.HardDeleteInventoryAsync(inventoryId, currentUserId);
                return Ok(inventory);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Inventory hard delete failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during inventory hard delete for ID {InventoryId}.", inventoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during hard deletion." });
            }
        }
    }
}
