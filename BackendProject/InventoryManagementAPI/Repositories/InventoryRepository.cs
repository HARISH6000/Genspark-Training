// Interfaces/IInventoryRepository.cs (as provided above in section 3)
// No changes here, just reminding it needs the GetByName method declaration.

// Repositories/InventoryRepository.cs
using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class InventoryRepository : Repository<int, Inventory>, IInventoryRepository
    {
        public InventoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Inventory?> Get(int key)
        {
            return await _applicationDbContext.Inventories.SingleOrDefaultAsync(i => i.InventoryId == key);
        }

        public override async Task<IEnumerable<Inventory>> GetAll()
        {
            return await _applicationDbContext.Inventories.ToListAsync();
        }

        
        public async Task<Inventory?> GetByName(string name)
        {
            return await _applicationDbContext.Inventories.SingleOrDefaultAsync(i => i.Name == name);
        }
    }
}
