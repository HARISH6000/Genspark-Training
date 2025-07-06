using InventoryManagementAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryProductRepository : IRepository<int, InventoryProduct>
    {
        Task<InventoryProduct?> GetByInventoryAndProductId(int inventoryId, int productId);
        Task<IEnumerable<InventoryProduct>> GetProductsForInventory(int inventoryId);
        IQueryable<InventoryProduct> GetProductsForInventoryAsQueryable(int inventoryId);
        Task<IEnumerable<InventoryProduct>> GetInventoriesForProduct(int productId);
        IQueryable<InventoryProduct> GetInventoriesForProductAsQueryable(int productId);
        Task<IEnumerable<InventoryProduct>> GetLowStockProducts(int inventoryId, int threshold);
        Task<IEnumerable<InventoryProduct>> GetProductsInInventoryByCategory(int inventoryId, int categoryId);
        
    }
}