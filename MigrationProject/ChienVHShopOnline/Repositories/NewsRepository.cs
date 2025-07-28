using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class NewsRepository : Repository<int, News>
    {
        public NewsRepository(ApplicationDbContext context) : base(context) {}

        public override async Task<News?> Get(int key)
        {
            return await _context.News
                .Include(n => n.User)
                .FirstOrDefaultAsync(n => n.NewsId == key);
        }

        public override async Task<IEnumerable<News>> GetAll()
        {
            return await _context.News
                .Include(n => n.User)
                .ToListAsync();
        }
    }
}
