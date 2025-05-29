using BankingApplication.Contexts;
using BankingApplication.Interfaces;
using BankingApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Repositories
{
    public  class UserRepository : Repository<int, User>
    {
        public UserRepository(BankContext bankContext) : base(bankContext)
        {
        }

        public override async Task<User> Get(int key)
        {
            var user = await _bankContext.Users.SingleOrDefaultAsync(u => u.Id == key);

            return user??throw new Exception("No user with the given ID");
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            var users = _bankContext.Users;
            if (users.Count() == 0)
                throw new Exception("No Users in the database");
            return (await users.ToListAsync());
        }
    }
}