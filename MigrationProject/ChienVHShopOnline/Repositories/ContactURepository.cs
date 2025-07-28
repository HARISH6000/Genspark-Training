using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class ContactURepository : Repository<int, ContactU>
    {
        public ContactURepository(ApplicationDbContext context) : base(context) {}

        public override async Task<ContactU?> Get(int key)
        {
            return await _context.ContactUs.FindAsync(key);
        }

        public override async Task<IEnumerable<ContactU>> GetAll()
        {
            return await _context.ContactUs.ToListAsync();
        }
    }
}
