using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto> AddProductAsync(AddProductDto productDto);
        Task<ProductResponseDto?> GetProductByIdAsync(int productId);
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool includeDeleted = false);
        Task<ProductResponseDto> UpdateProductAsync(UpdateProductDto productDto);
        Task<ProductResponseDto> SoftDeleteProductAsync(int productId); 
        Task<ProductResponseDto> HardDeleteProductAsync(int productId);
    }
}
