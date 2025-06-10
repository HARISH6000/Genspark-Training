using InventoryManagementAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IInventoryManagerRepository : IRepository<int, InventoryManager>
    {
        Task<InventoryManager?> GetByInventoryAndManagerId(int inventoryId, int managerId);
        Task<IEnumerable<InventoryManager>> GetManagersForInventory(int inventoryId);
        Task<IEnumerable<InventoryManager>> GetInventoriesManagedByManager(int managerId);
        Task<IEnumerable<InventoryManager>> GetAssignmentsByInventoryId(int inventoryId);
        Task<IEnumerable<InventoryManager>> GetAssignmentsByManagerId(int managerId);
    }
}