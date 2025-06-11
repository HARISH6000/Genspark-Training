using InventoryManagementAPI.Models;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface ICategoryRepository : IRepository<int, Category>
    {
        Task<Category?> GetByName(string categoryName);
    }
}
