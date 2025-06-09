using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class InventoryProductRepository : Repository<int, InventoryProduct>
    {
        public InventoryProductRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<InventoryProduct> Get(int key)
        {
            return await _applicationDbContext.InventoryProducts.SingleOrDefaultAsync(ip => ip.Id == key);
        }

        public override async Task<IEnumerable<InventoryProduct>> GetAll()
        {
            return await _applicationDbContext.InventoryProducts.ToListAsync();
        }
            
    }
}