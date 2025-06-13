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
    public class InventoryManagersController : ControllerBase
    {
        private readonly IInventoryManagerService _inventoryManagerService;
        private readonly IInventoryService _inventoryService;
        private readonly IUserService _userService;
        private readonly ILogger<InventoryManagersController> _logger;

        public InventoryManagersController(IInventoryManagerService inventoryManagerService, IInventoryService inventoryService, IUserService userService, ILogger<InventoryManagersController> logger)
        {
            _inventoryManagerService = inventoryManagerService;
            _inventoryService = inventoryService;
            _userService = userService;
            _logger = logger;
        }

        
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(InventoryManagerResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AssignManagerToInventory([FromBody] AssignRemoveInventoryManagerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId(); 
                var newAssignment = await _inventoryManagerService.AssignManagerToInventoryAsync(dto, currentUserId); // Pass currentUserId
                return StatusCode(StatusCodes.Status201Created, newAssignment);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Assignment failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "Assignment conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Assignment failed due to invalid role: {Message}", ex.Message);
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during manager assignment.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during manager assignment." });
            }
        }

        
        [HttpDelete("remove")]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InventoryManagerResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveManagerFromInventory([FromBody] AssignRemoveInventoryManagerDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId(); 
                var removedAssignment = await _inventoryManagerService.RemoveManagerFromInventoryAsync(dto, currentUserId); 
                return Ok(removedAssignment);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Removal failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during manager removal.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during manager removal." });
            }
        }

        
        [HttpGet("by-inventory/{inventoryId}")]
        [Authorize(Roles = "Admin,Manager")] 
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ManagerForInventoryResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetManagersForInventory(int inventoryId, [FromQuery] string? sortBy = null)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(inventoryId);
                if (inventory == null || inventory.IsDeleted)
                {
                    return NotFound(new { message = $"Inventory with ID {inventoryId} not found or is deleted." });
                }
                var managers = await _inventoryManagerService.GetManagersForInventoryAsync(inventoryId, sortBy);
                if (!managers.Any())
                {
                    return NotFound(new { message = $"Inventory with ID {inventoryId} has no assigned managers." });
                }
                return Ok(managers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving managers for inventory ID {InventoryId}.", inventoryId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        
        [HttpGet("by-manager/{managerId}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryManagedByManagerResponseDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoriesManagedByManager(int managerId, [FromQuery] string? sortBy = null)
        {
            try
            {
                var manager = await _userService.GetUserByIdAsync(managerId);
                if (manager == null || manager.IsDeleted)
                {
                    return NotFound(new { message = $"Manager with ID {managerId} not found or is deleted." });
                }
                var inventories = await _inventoryManagerService.GetInventoriesManagedByManagerAsync(managerId, sortBy);
                if (!inventories.Any())
                {
                    return NotFound(new { message = $"Manager with ID {managerId} manages no active inventories." });
                }
                return Ok(inventories);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "Request for inventories by manager failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving inventories managed by user ID {ManagerId}.", managerId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        
        [HttpGet]
        [Authorize(Roles = "Admin")] 
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryManagerResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllAssignments()
        {
            try
            {
                var assignments = await _inventoryManagerService.GetAllAssignmentsAsync();
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all inventory-manager assignments.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }
    }
}
