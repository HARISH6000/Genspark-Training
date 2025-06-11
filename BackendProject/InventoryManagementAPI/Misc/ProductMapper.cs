using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class ProductMapper
    {
        // Maps an AddProductDto to a new Product model.
        public static Product ToProduct(AddProductDto dto)
        {
            return new Product
            {
                SKU = dto.SKU,
                ProductName = dto.ProductName,
                Description = dto.Description,
                UnitPrice = dto.UnitPrice,
                CategoryId = dto.CategoryId,
                IsDeleted = false
            };
        }

        // Maps an UpdateProductDto to an existing Product model.        
        public static void ToProduct(UpdateProductDto dto, Product product)
        {
            product.SKU = dto.SKU;
            product.ProductName = dto.ProductName;
            product.Description = dto.Description;
            product.UnitPrice = dto.UnitPrice;
            product.CategoryId = dto.CategoryId;
        }

        
        // Maps a Product model to a ProductResponseDto.
        public static ProductResponseDto ToProductResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                Description = product.Description,
                UnitPrice = product.UnitPrice,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName ?? "Unknown",
                IsDeleted = product.IsDeleted
            };
        }
    }
}
