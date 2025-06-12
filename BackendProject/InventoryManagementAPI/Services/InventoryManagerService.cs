using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using InventoryManagementAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json; 

namespace InventoryManagementAPI.Services
{
    public class InventoryManagerService : IInventoryManagerService
    {
        private readonly IInventoryManagerRepository _inventoryManagerRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditLogService _auditLogService; 

        public InventoryManagerService(
            IInventoryManagerRepository inventoryManagerRepository,
            IInventoryRepository inventoryRepository,
            IUserRepository userRepository,
            IAuditLogService auditLogService)
        {
            _inventoryManagerRepository = inventoryManagerRepository;
            _inventoryRepository = inventoryRepository;
            _userRepository = userRepository;
            _auditLogService = auditLogService; 
        }

        public async Task<InventoryManagerResponseDto> AssignManagerToInventoryAsync(AssignRemoveInventoryManagerDto dto, int? currentUserId)
        {
            var inventory = await _inventoryRepository.Get(dto.InventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {dto.InventoryId} not found or is deleted.");
            }

            var managerUser = await _userRepository.Get(dto.ManagerId);
            if (managerUser == null || managerUser.IsDeleted)
            {
                throw new NotFoundException($"Manager (User ID {dto.ManagerId}) not found or is deleted.");
            }
            if (managerUser.Role?.RoleName != "Manager" && managerUser.Role?.RoleName != "Admin")
            {
                throw new InvalidOperationException($"User '{managerUser.Username}' (ID: {managerUser.UserId}) does not have the 'Manager' or 'Admin' role and cannot be assigned as an inventory manager.");
            }

            var existingAssignment = await _inventoryManagerRepository.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId);
            if (existingAssignment != null)
            {
                throw new ConflictException($"Manager '{managerUser.Username}' is already assigned to inventory '{inventory.Name}'.");
            }

            var newAssignment = InventoryManagerMapper.ToInventoryManager(dto);
            newAssignment.Inventory = inventory;
            newAssignment.Manager = managerUser;

            try
            {
                var addedAssignment = await _inventoryManagerRepository.Add(newAssignment);
                
                // AUDIT LOGGING: INSERT Assignment
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "InventoryManagers",
                    RecordId = addedAssignment.Id.ToString(),
                    ActionType = "INSERT",
                    NewValues = addedAssignment
                });

                return InventoryManagerMapper.ToInventoryManagerResponseDto(addedAssignment);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    throw new ConflictException($"Manager '{managerUser.Username}' is already assigned to inventory '{inventory.Name}'. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<InventoryManagerResponseDto> RemoveManagerFromInventoryAsync(AssignRemoveInventoryManagerDto dto, int? currentUserId)
        {
            var assignmentToDelete = await _inventoryManagerRepository.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId);
            if (assignmentToDelete == null)
            {
                throw new NotFoundException($"Assignment not found for Inventory ID {dto.InventoryId} and Manager ID {dto.ManagerId}.");
            }

            // Capture old state before deletion
            var oldAssignmentSnapshot = JsonSerializer.Deserialize<InventoryManager>(JsonSerializer.Serialize(assignmentToDelete));

            var deletedAssignment = await _inventoryManagerRepository.Delete(assignmentToDelete.Id);
            
            // AUDIT LOGGING: DELETE Assignment
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "InventoryManagers",
                RecordId = deletedAssignment.Id.ToString(),
                ActionType = "DELETE",
                OldValues = oldAssignmentSnapshot,
                NewValues = null 
            });

            return InventoryManagerMapper.ToInventoryManagerResponseDto(deletedAssignment);
        }

        public async Task<IEnumerable<ManagerForInventoryResponseDto>> GetManagersForInventoryAsync(int inventoryId, string? sortBy = null)
        {
            var inventory = await _inventoryRepository.Get(inventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found or is deleted.");
            }

            var assignments = await _inventoryManagerRepository.GetManagersForInventory(inventoryId);

            var activeManagers = assignments.Where(im => im.Manager != null && !im.Manager.IsDeleted)
                                            .Select(im => im.Manager!)
                                            .DistinctBy(u => u.UserId);

            var responseDto = activeManagers.Select(m => InventoryManagerMapper.ToManagerForInventoryResponseDto(m));
            

            return SortHelper.ApplySorting(responseDto, sortBy);
        }

        public async Task<IEnumerable<InventoryManagedByManagerResponseDto>> GetInventoriesManagedByManagerAsync(int managerId, string? sortBy = null)
        {
            var managerUser = await _userRepository.Get(managerId);
            if (managerUser == null || managerUser.IsDeleted)
            {
                throw new NotFoundException($"Manager (User ID {managerId}) not found or is deleted.");
            }

            var assignments = await _inventoryManagerRepository.GetInventoriesManagedByManager(managerId);

            var activeInventories = assignments.Where(im => im.Inventory != null && !im.Inventory.IsDeleted)
                                               .Select(im => im.Inventory!)
                                               .DistinctBy(i => i.InventoryId);

            var responseDto = activeInventories.Select(i => InventoryManagerMapper.ToInventoryManagedByManagerResponseDto(i));
            
            return SortHelper.ApplySorting(responseDto, sortBy);
        }

        public async Task<IEnumerable<InventoryManagerResponseDto>> GetAllAssignmentsAsync()
        {
            var assignments = await _inventoryManagerRepository.GetAll();
            return assignments.Select(a => InventoryManagerMapper.ToInventoryManagerResponseDto(a));
        }
    }
}
