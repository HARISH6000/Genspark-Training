using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions; // For custom exceptions
using InventoryManagementAPI.Mappers; // For InventoryMapper
using Microsoft.EntityFrameworkCore; // For DbUpdateException

namespace InventoryManagementAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        // private readonly IAuditLogService _auditLogService; // To be implemented later

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
            // _auditLogService = auditLogService;
        }

        public async Task<InventoryResponseDto> AddInventoryAsync(AddInventoryDto inventoryDto)
        {
            
            var existingInventory = await _inventoryRepository.GetByName(inventoryDto.Name);
            if (existingInventory != null)
            {
                throw new ConflictException($"Inventory with name '{inventoryDto.Name}' already exists.");
            }

            
            var newInventory = InventoryMapper.ToInventory(inventoryDto);

            
            try
            {
                var addedInventory = await _inventoryRepository.Add(newInventory);
                // _auditLogService.LogActionAsync(currentUserId, "Inventory", addedInventory.InventoryId, "INSERT", null, addedInventory);
                return InventoryMapper.ToInventoryResponseDto(addedInventory);
            }
            catch (DbUpdateException ex) // Catch potential database unique constraint violations
            {
                
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("Name") || ex.InnerException.Message.Contains("name"))
                        throw new ConflictException($"Inventory with name '{inventoryDto.Name}' already exists. (Database constraint)");
                }
                throw; 
            }
        }

        public async Task<InventoryResponseDto?> GetInventoryByIdAsync(int inventoryId)
        {
            var inventory = await _inventoryRepository.Get(inventoryId);
            if (inventory == null)
            {
                return null;
            }
            return InventoryMapper.ToInventoryResponseDto(inventory);
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllInventoriesAsync(bool includeDeleted = false)
        {
            var inventories = await _inventoryRepository.GetAll();

            if (!includeDeleted)
            {
                inventories = inventories.Where(i => !i.IsDeleted);
            }

            return inventories.Select(i => InventoryMapper.ToInventoryResponseDto(i));
        }

        public async Task<InventoryResponseDto> UpdateInventoryAsync(UpdateInventoryDto inventoryDto)
        {
            var existingInventory = await _inventoryRepository.Get(inventoryDto.InventoryId);
            if (existingInventory == null)
            {
                throw new NotFoundException($"Inventory with ID {inventoryDto.InventoryId} not found.");
            }

            // Check for Name uniqueness if Name is being changed to an already existing one that's not its own
            if (existingInventory.Name != inventoryDto.Name)
            {
                var inventoryWithSameName = await _inventoryRepository.GetByName(inventoryDto.Name);
                if (inventoryWithSameName != null && inventoryWithSameName.InventoryId != existingInventory.InventoryId)
                {
                    throw new ConflictException($"Inventory with name '{inventoryDto.Name}' already exists.");
                }
            }

            
            InventoryMapper.ToInventory(inventoryDto, existingInventory);

            try
            {
                var updatedInventory = await _inventoryRepository.Update(inventoryDto.InventoryId, existingInventory);
                // _auditLogService.LogActionAsync(currentUserId, "Inventory", updatedInventory.InventoryId, "UPDATE", oldValues, updatedInventory);
                return InventoryMapper.ToInventoryResponseDto(updatedInventory);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("Name") || ex.InnerException.Message.Contains("name"))
                        throw new ConflictException($"Inventory with name '{inventoryDto.Name}' already exists. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<InventoryResponseDto> SoftDeleteInventoryAsync(int inventoryId)
        {
            var inventoryToDelete = await _inventoryRepository.Get(inventoryId);
            if (inventoryToDelete == null)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found.");
            }

            if (inventoryToDelete.IsDeleted)
            {
                return InventoryMapper.ToInventoryResponseDto(inventoryToDelete); 
            }

            inventoryToDelete.IsDeleted = true; // Set IsDeleted flag
            var deletedInventory = await _inventoryRepository.Update(inventoryId, inventoryToDelete);
            // _auditLogService.LogActionAsync(currentUserId, "Inventory", inventoryId, "SOFT_DELETE", inventoryToDelete, deletedInventory);
            return InventoryMapper.ToInventoryResponseDto(deletedInventory);
        }

        public async Task<InventoryResponseDto> HardDeleteInventoryAsync(int inventoryId)
        {
            
            var inventoryToDelete = await _inventoryRepository.Get(inventoryId);
            if (inventoryToDelete == null)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found.");
            }

            var deletedInventory = await _inventoryRepository.Delete(inventoryId);
            // _auditLogService.LogActionAsync(currentUserId, "Inventory", inventoryId, "HARD_DELETE", inventoryToDelete, null);
            return InventoryMapper.ToInventoryResponseDto(deletedInventory);
        }
    }
}
