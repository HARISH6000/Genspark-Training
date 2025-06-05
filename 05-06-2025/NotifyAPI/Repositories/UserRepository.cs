using NotifyAPI.Contexts;
using NotifyAPI.Interfaces;
using NotifyAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace NotifyAPI.Repositories
{
    public class UserRepository : Repository<string, User>
    {
        public UserRepository(NotifyContext context):base(context)
        {
            
        }
        public override async Task<User> Get(string key)
        {
            return await _notifyContext.Users.SingleOrDefaultAsync(u => u.Username == key);
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            return await _notifyContext.Users.ToListAsync();
        }
            
    }
}