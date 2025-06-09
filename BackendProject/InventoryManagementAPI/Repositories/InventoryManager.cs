using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class InventoryManagerRepository : Repository<int, InventoryManager>
    {
        public InventoryManagerRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<InventoryManager> Get(int key)
        {
            return await _applicationDbContext.InventoryManagers.SingleOrDefaultAsync(im => im.Id == key);
        }

        public override async Task<IEnumerable<InventoryManager>> GetAll()
        {
            return await _applicationDbContext.InventoryManagers.ToListAsync();
        }
            
    }
}