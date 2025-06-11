using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Repositories
{
    public class CategoryRepository : Repository<int, Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        
        public async Task<Category?> GetByName(string categoryName)
        {
            return await _applicationDbContext.Categories
                                              .SingleOrDefaultAsync(c => c.CategoryName == categoryName);
        }

        
        public override async Task<Category?> Get(int key)
        {
            
            return await _applicationDbContext.Categories
                                              .SingleOrDefaultAsync(c => c.CategoryId == key);
        }

        
        public override async Task<IEnumerable<Category>> GetAll()
        {
            return await _applicationDbContext.Categories.ToListAsync();
        }
    }
}
