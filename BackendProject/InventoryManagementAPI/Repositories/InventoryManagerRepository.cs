using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Repositories
{
    public class InventoryManagerRepository : Repository<int, InventoryManager>, IInventoryManagerRepository
    {
        public InventoryManagerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<InventoryManager> Get(int key)
        {
            return await _applicationDbContext.InventoryManagers
                                              .Include(im => im.Inventory)
                                              .Include(im => im.Manager)
                                                  .ThenInclude(m => m.Role)
                                              .SingleOrDefaultAsync(im => im.Id == key);
        }


        public override async Task<IEnumerable<InventoryManager>> GetAll()
        {
            return await _applicationDbContext.InventoryManagers
                                              .Include(im => im.Inventory)
                                              .Include(im => im.Manager)
                                                  .ThenInclude(m => m.Role)
                                              .ToListAsync();
        }


        public async Task<InventoryManager?> GetByInventoryAndManagerId(int inventoryId, int managerId)
        {
            return await _applicationDbContext.InventoryManagers
                                              .Include(im => im.Inventory)
                                              .Include(im => im.Manager)
                                                  .ThenInclude(m => m.Role)
                                              .SingleOrDefaultAsync(im => im.InventoryId == inventoryId && im.ManagerId == managerId);
        }


        public async Task<IEnumerable<InventoryManager>> GetManagersForInventory(int inventoryId)
        {
            return await _applicationDbContext.InventoryManagers
                                              .Include(im => im.Manager)
                                                  .ThenInclude(m => m.Role)
                                              .Where(im => im.InventoryId == inventoryId)
                                              .ToListAsync();
        }


        public async Task<IEnumerable<InventoryManager>> GetInventoriesManagedByManager(int managerId)
        {
            return await _applicationDbContext.InventoryManagers
                                              .Include(im => im.Inventory) // Eager load the Inventory
                                              .Where(im => im.ManagerId == managerId)
                                              .ToListAsync();
        }
        
        public async Task<IEnumerable<InventoryManager>> GetAssignmentsByInventoryId(int inventoryId)
        {
            return await _applicationDbContext.InventoryManagers
                                              .Where(im => im.InventoryId == inventoryId)
                                              .ToListAsync();
        }

        public async Task<IEnumerable<InventoryManager>> GetAssignmentsByManagerId(int managerId)
        {
            return await _applicationDbContext.InventoryManagers
                                              .Where(im => im.ManagerId == managerId)
                                              .ToListAsync();
        }

        public async Task<bool> IsUserManagerOfInventory(int userId, int inventoryId)
        {
            return await _applicationDbContext.InventoryManagers
                                             .AnyAsync(im => im.ManagerId == userId && im.InventoryId == inventoryId);
        }
    }
}
