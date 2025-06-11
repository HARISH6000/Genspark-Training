// Interfaces/IProductService.cs
using InventoryManagementAPI.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> AddProductAsync(AddProductDto productDto, int? currentUserId);
        Task<ProductResponseDto?> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool includeDeleted = false);
        Task<ProductResponseDto> UpdateProductAsync(UpdateProductDto productDto, int? currentUserId);
        Task<ProductResponseDto> SoftDeleteProductAsync(int productId, int? currentUserId);
        Task<ProductResponseDto> HardDeleteProductAsync(int productId, int? currentUserId);
    }
}
