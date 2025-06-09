using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class ProductRepository : Repository<int, Product>
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<Product> Get(int key)
        {
            return await _applicationDbContext.Products.SingleOrDefaultAsync(p => p.ProductId == key);
        }

        public override async Task<IEnumerable<Product>> GetAll()
        {
            return await _applicationDbContext.Products.ToListAsync();
        }
            
    }
}