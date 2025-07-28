using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class ModelRepository : Repository<int, Model>
    {
        public ModelRepository(ApplicationDbContext context) : base(context) {}

        public override async Task<Model?> Get(int key)
        {
            return await _context.Models
                .Include(m => m.Products)
                .FirstOrDefaultAsync(m => m.ModelId == key);
        }

        public override async Task<IEnumerable<Model>> GetAll()
        {
            return await _context.Models
                .Include(m => m.Products)
                .ToListAsync();
        }
    }
}
