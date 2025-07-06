
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Interfaces
{
    public interface IProductRepository : IRepository<int, Product>
    {
        Task<Product?> GetBySKU(string sku);

        IQueryable<Product> GetAllAsQueryable();
    }
}