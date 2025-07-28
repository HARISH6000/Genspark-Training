using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class CategoryRepository : Repository<int, Category>
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) {}

        public override async Task<Category?> Get(int key)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == key);
        }

        public override async Task<IEnumerable<Category>> GetAll()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
        }
    }
}
