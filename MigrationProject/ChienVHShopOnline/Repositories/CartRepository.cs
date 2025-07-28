using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class CartRepository : Repository<int, Cart>
    {
        public CartRepository(ApplicationDbContext context) : base(context) {}

        public override async Task<Cart?> Get(int key)
        {
            return await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.Id == key);
        }

        public override async Task<IEnumerable<Cart>> GetAll()
        {
            return await _context.Carts
                .Include(c => c.Product)
                .ToListAsync();
        }
    }
}
