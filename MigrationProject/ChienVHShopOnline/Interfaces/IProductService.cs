using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryId);
    Task<ProductDto> CreateProductAsync(ProductCreateDto dto);
    Task<bool> UpdateProductAsync(ProductUpdateDto dto);
    Task<bool> DeleteProductAsync(int id);
}
