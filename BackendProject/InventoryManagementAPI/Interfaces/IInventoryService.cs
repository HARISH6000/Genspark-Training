// Interfaces/IInventoryService.cs
using InventoryManagementAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto> AddInventoryAsync(AddInventoryDto inventoryDto);
        Task<InventoryResponseDto?> GetInventoryByIdAsync(int inventoryId);
        Task<IEnumerable<InventoryResponseDto>> GetAllInventoriesAsync(bool includeDeleted = false);
        Task<InventoryResponseDto> UpdateInventoryAsync(UpdateInventoryDto inventoryDto);
        Task<InventoryResponseDto> SoftDeleteInventoryAsync(int inventoryId);
        Task<InventoryResponseDto> HardDeleteInventoryAsync(int inventoryId); // Use with caution
    }
}
