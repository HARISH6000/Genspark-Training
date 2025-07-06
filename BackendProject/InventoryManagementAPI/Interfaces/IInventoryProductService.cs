using InventoryManagementAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryProductService
    {
        Task<InventoryProductResponseDto> AddInventoryProductAsync(AddInventoryProductDto dto, int? currentUserId);
        Task<InventoryProductResponseDto> IncreaseProductQuantityAsync(AdjustProductQuantityDto dto, int? currentUserId);
        Task<InventoryProductResponseDto> DecreaseProductQuantityAsync(AdjustProductQuantityDto dto, int? currentUserId);
        Task<InventoryProductResponseDto> SetProductQuantityAsync(SetProductQuantityDto dto, int? currentUserId);
        Task<InventoryProductResponseDto> DeleteInventoryProductAsync(int inventoryProductId, int? currentUserId);
        Task<InventoryProductResponseDto> UpdateMinStockQuantityAsync(UpdateInventoryProductMinStockDto dto, int? currentUserId);

        Task<InventoryProductResponseDto?> GetInventoryProductByIdAsync(int inventoryProductId);
        Task<InventoryProductResponseDto?> GetInventoryProductByInventoryAndProductIdAsync(int inventoryId, int productId);
        Task<IEnumerable<InventoryProductResponseDto>> GetAllInventoryProductsAsync(string? sortBy = null);
        Task<IEnumerable<ProductInInventoryResponseDto>> GetProductsInInventoryAsync(int inventoryId, string? sortBy = null);
        Task<PaginationResponse<ProductInInventoryResponseDto>> GetProductsInInventoryAsync(int inventoryId, int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null);
        Task<IEnumerable<ProductInInventoryResponseDto>> GetProductsInInventoryByCategoryAsync(int inventoryId, int categoryId, string? sortBy = null);
        Task<IEnumerable<InventoryForProductResponseDto>> GetInventoriesForProductAsync(int productId, string? sortBy = null);
        Task<PaginationResponse<InventoryForProductResponseDto>> GetInventoriesForProductAsync(int productId, int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null);
        Task<IEnumerable<InventoryForProductResponseDto>> GetInventoriesForProductBySKUAsync(string sku, string? sortBy = null);
        Task<IEnumerable<ProductInInventoryResponseDto>> GetLowStockProductsInInventoryAsync(int inventoryId, int threshold, string? sortBy = null);
    }
}
