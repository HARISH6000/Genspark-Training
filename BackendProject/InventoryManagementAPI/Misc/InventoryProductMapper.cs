using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class InventoryProductMapper
    {
        public static InventoryProduct ToInventoryProduct(AddInventoryProductDto dto)
        {
            return new InventoryProduct
            {
                InventoryId = dto.InventoryId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                MinStockQuantity = dto.MinStockQuantity
            };
        }

        public static InventoryProductResponseDto ToInventoryProductResponseDto(InventoryProduct inventoryProduct)
        {
            return new InventoryProductResponseDto
            {
                Id = inventoryProduct.Id,
                InventoryId = inventoryProduct.InventoryId,
                InventoryName = inventoryProduct.Inventory?.Name ?? "Unknown Inventory",
                InventoryLocation = inventoryProduct.Inventory?.Location ?? "Unknown Location",
                ProductId = inventoryProduct.ProductId,
                ProductName = inventoryProduct.Product?.ProductName ?? "Unknown Product",
                ProductSKU = inventoryProduct.Product?.SKU ?? "Unknown SKU",
                Quantity = inventoryProduct.Quantity,
                MinStockQuantity = inventoryProduct.MinStockQuantity
            };
        }

        public static ProductInInventoryResponseDto ToProductInInventoryResponseDto(InventoryProduct inventoryProduct)
        {
            return new ProductInInventoryResponseDto
            {
                Id = inventoryProduct.Id,
                ProductId = inventoryProduct.ProductId,
                ProductName = inventoryProduct.Product?.ProductName ?? "Unknown Product",
                SKU = inventoryProduct.Product?.SKU ?? "Unknown SKU",
                Description = inventoryProduct.Product?.Description,
                UnitPrice = inventoryProduct.Product?.UnitPrice ?? 0.00M,
                CategoryId = inventoryProduct.Product?.CategoryId ?? 0,
                CategoryName = inventoryProduct.Product?.Category?.CategoryName ?? "Unknown Category",
                QuantityInInventory = inventoryProduct.Quantity,
                MinStockQuantity = inventoryProduct.MinStockQuantity,
            };
        }

        public static InventoryForProductResponseDto ToInventoryForProductResponseDto(InventoryProduct inventoryProduct)
        {
            return new InventoryForProductResponseDto
            {
                InventoryId = inventoryProduct.InventoryId,
                InventoryName = inventoryProduct.Inventory?.Name ?? "Unknown Inventory",
                InventoryLocation = inventoryProduct.Inventory?.Location ?? "Unknown Location",
                QuantityInInventory = inventoryProduct.Quantity,
                MinStockQuantity = inventoryProduct.MinStockQuantity
            };
        }
    }
}
