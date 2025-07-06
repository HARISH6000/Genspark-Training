using InventoryManagementAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto> AddInventoryAsync(AddInventoryDto inventoryDto, int? currentUserId);
        Task<InventoryResponseDto?> GetInventoryByIdAsync(int inventoryId);
        Task<IEnumerable<InventoryResponseDto>> GetAllInventoriesAsync(bool includeDeleted = false);
        Task<PaginationResponse<InventoryResponseDto>> GetAllInventoriesAsync(int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null,
            bool includeDeleted = false);
        Task<InventoryResponseDto> UpdateInventoryAsync(UpdateInventoryDto inventoryDto, int? currentUserId);
        Task<InventoryResponseDto> SoftDeleteInventoryAsync(int inventoryId, int? currentUserId);
        Task<InventoryResponseDto> HardDeleteInventoryAsync(int inventoryId, int? currentUserId);
    }
}
