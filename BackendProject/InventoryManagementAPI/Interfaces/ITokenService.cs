using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
    }
}