using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class InventoryRepository : Repository<int, Inventory>
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<Inventory> Get(int key)
        {
            return await _applicationDbContext.Inventories.SingleOrDefaultAsync(i => i.InventoryId == key);
        }

        public override async Task<IEnumerable<Inventory>> GetAll()
        {
            return await _applicationDbContext.Inventories.ToListAsync();
        }
            
    }
}