using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class CategoryRepository : Repository<int, Category>
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<Category> Get(int key)
        {
            return await _applicationDbContext.Categories.SingleOrDefaultAsync(c => c.CategoryId == key);
        }

        public override async Task<IEnumerable<Category>> GetAll()
        {
            return await _applicationDbContext.Categories.ToListAsync();
        }
            
    }
}