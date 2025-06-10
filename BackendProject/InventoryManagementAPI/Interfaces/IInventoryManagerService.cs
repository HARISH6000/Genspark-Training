// Interfaces/IInventoryManagerService.cs
using InventoryManagementAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryManagerService
    {
        Task<InventoryManagerResponseDto> AssignManagerToInventoryAsync(AssignRemoveInventoryManagerDto dto);
        Task<InventoryManagerResponseDto> RemoveManagerFromInventoryAsync(AssignRemoveInventoryManagerDto dto);
        Task<IEnumerable<ManagerForInventoryResponseDto>> GetManagersForInventoryAsync(int inventoryId, string? sortBy = null);
        Task<IEnumerable<InventoryManagedByManagerResponseDto>> GetInventoriesManagedByManagerAsync(int managerId, string? sortBy = null);
        Task<IEnumerable<InventoryManagerResponseDto>> GetAllAssignmentsAsync(); // To list all assignments
    }
}