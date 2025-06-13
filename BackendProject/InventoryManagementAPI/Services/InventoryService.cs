using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;

namespace InventoryManagementAPI.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IInventoryManagerRepository _inventoryManagerRepository;
        private readonly IAuditLogService _auditLogService;

        public InventoryService(IInventoryRepository inventoryRepository, 
                                IInventoryManagerRepository inventoryManagerRepository,
                                IAuditLogService auditLogService)
        {
            _inventoryRepository = inventoryRepository;
            _inventoryManagerRepository = inventoryManagerRepository;
            _auditLogService = auditLogService;
        }

        public async Task<InventoryResponseDto> AddInventoryAsync(AddInventoryDto inventoryDto, int? currentUserId)
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
                
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Inventories",
                    RecordId = addedInventory.InventoryId.ToString(),
                    ActionType = "INSERT",
                    NewValues = addedInventory
                });

                return InventoryMapper.ToInventoryResponseDto(addedInventory);
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

        public async Task<InventoryResponseDto> UpdateInventoryAsync(UpdateInventoryDto inventoryDto, int? currentUserId)
        {
            var existingInventory = await _inventoryRepository.Get(inventoryDto.InventoryId);
            if (existingInventory == null)
            {
                throw new NotFoundException($"Inventory with ID {inventoryDto.InventoryId} not found.");
            }

            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(existingInventory)); 

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
                
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Inventories",
                    RecordId = updatedInventory.InventoryId.ToString(),
                    ActionType = "UPDATE",
                    OldValues = oldInventorySnapshot,
                    NewValues = updatedInventory
                });

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

        public async Task<InventoryResponseDto> SoftDeleteInventoryAsync(int inventoryId, int? currentUserId)
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

            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(inventoryToDelete));

            inventoryToDelete.IsDeleted = true; 
            var deletedInventory = await _inventoryRepository.Update(inventoryId, inventoryToDelete);
            
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Inventories",
                RecordId = inventoryId.ToString(),
                ActionType = "SOFT_DELETE",
                OldValues = oldInventorySnapshot,
                NewValues = deletedInventory
            });

            var associatedAssignments = await _inventoryManagerRepository.GetAssignmentsByInventoryId(inventoryId);
            foreach (var assignment in associatedAssignments)
            {
                await _inventoryManagerRepository.Delete(assignment.Id);
            }
            return InventoryMapper.ToInventoryResponseDto(deletedInventory);
        }

        public async Task<InventoryResponseDto> HardDeleteInventoryAsync(int inventoryId, int? currentUserId)
        {
            var inventoryToDelete = await _inventoryRepository.Get(inventoryId);
            if (inventoryToDelete == null)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found.");
            }

            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(inventoryToDelete));

            var deletedInventory = await _inventoryRepository.Delete(inventoryId);
            
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Inventories",
                RecordId = inventoryId.ToString(),
                ActionType = "HARD_DELETE",
                OldValues = oldInventorySnapshot, 
                NewValues = null 
            });
            
            return InventoryMapper.ToInventoryResponseDto(deletedInventory);
        }
    }
}
