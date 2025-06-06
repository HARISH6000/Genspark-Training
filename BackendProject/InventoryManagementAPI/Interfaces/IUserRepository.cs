using InventoryManagementAPI.Models;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IUserRepository : IRepository<int, User>
    {
        Task<User> GetByUsername(string username);
    }
}