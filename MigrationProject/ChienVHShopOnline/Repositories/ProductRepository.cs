using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class ProductRepository : Repository<int, Product>
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Product?> Get(int key)
        {
            return await _context.Products
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Model)
                .Include(p => p.OrderDetails)
                .FirstOrDefaultAsync(p => p.ProductId == key);
        }

        public override async Task<IEnumerable<Product>> GetAll()
        {
            return await _context.Products
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Color)
                .Include(p => p.Model)
                .Include(p => p.OrderDetails)
                .ToListAsync();
        }
    }
}
