using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryRepository : IRepository<int, Inventory>
    {
        Task<Inventory?> GetByName(string name); 
    }
}