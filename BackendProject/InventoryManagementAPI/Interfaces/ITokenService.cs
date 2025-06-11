using InventoryManagementAPI.Models;
using System.Security.Claims;

namespace InventoryManagementAPI.Interfaces
{
    public interface ITokenService
    {
        public string GenerateJwtToken(User user);
        string GenerateJwtToken(IEnumerable<Claim> claims);
    }
}