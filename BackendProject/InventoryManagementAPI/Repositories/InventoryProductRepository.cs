using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Repositories
{
    public class InventoryProductRepository : Repository<int, InventoryProduct>, IInventoryProductRepository
    {
        public InventoryProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<InventoryProduct?> Get(int key)
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Inventory) 
                                              .Include(ip => ip.Product)
                                                  .ThenInclude(p => p.Category)
                                              .SingleOrDefaultAsync(ip => ip.Id == key);
        }

        public override async Task<IEnumerable<InventoryProduct>> GetAll()
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Inventory)
                                              .Include(ip => ip.Product)
                                                  .ThenInclude(p => p.Category)
                                              .ToListAsync();
        }

        public async Task<InventoryProduct?> GetByInventoryAndProductId(int inventoryId, int productId)
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Inventory)
                                              .Include(ip => ip.Product)
                                                  .ThenInclude(p => p.Category)
                                              .SingleOrDefaultAsync(ip => ip.InventoryId == inventoryId && ip.ProductId == productId);
        }

        public async Task<IEnumerable<InventoryProduct>> GetProductsForInventory(int inventoryId)
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Product)
                                                  .ThenInclude(p => p.Category)
                                              .Where(ip => ip.InventoryId == inventoryId)
                                              .ToListAsync();
        }

        public async Task<IEnumerable<InventoryProduct>> GetInventoriesForProduct(int productId)
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Inventory)
                                              .Where(ip => ip.ProductId == productId)
                                              .ToListAsync();
        }

        public async Task<IEnumerable<InventoryProduct>> GetLowStockProducts(int inventoryId, int threshold)
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Product)
                                                  .ThenInclude(p => p.Category)
                                              .Where(ip => ip.InventoryId == inventoryId && ip.Quantity <= threshold)
                                              .ToListAsync();
        }

        public async Task<IEnumerable<InventoryProduct>> GetProductsInInventoryByCategory(int inventoryId, int categoryId)
        {
            return await _applicationDbContext.InventoryProducts
                                              .Include(ip => ip.Product)
                                                  .ThenInclude(p => p.Category)
                                              .Where(ip => ip.InventoryId == inventoryId && ip.Product!.CategoryId == categoryId)
                                              .ToListAsync();
        }
    }
}
