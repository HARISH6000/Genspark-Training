using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Interfaces
{
    public interface ITokenService
    {
        public string GenerateJwtToken(User user);
    }
}