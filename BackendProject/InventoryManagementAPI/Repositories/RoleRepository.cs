using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class RoleRepository : Repository<int, Role>
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<Role> Get(int key)
        {
            return await _applicationDbContext.Roles.SingleOrDefaultAsync(r => r.RoleId == key);
        }

        public override async Task<IEnumerable<Role>> GetAll()
        {
            return await _applicationDbContext.Roles.ToListAsync();
        }
            
    }
}