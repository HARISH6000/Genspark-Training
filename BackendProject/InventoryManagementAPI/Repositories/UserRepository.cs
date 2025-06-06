using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class UserRepository : Repository<int, User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<User> Get(int key)
        {
            return await _applicationDbContext.Users.SingleOrDefaultAsync(u => u.UserId == key);
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            return await _applicationDbContext.Users.ToListAsync();
        }

        public async Task<User> GetByUsername(string username)
        {
            return await _applicationDbContext.Users.SingleOrDefaultAsync(u => u.Username == username);
        }
            
    }
}