using InventoryManagementAPI.Models;
using System.Security.Claims;
using System.Collections.Generic; // Add this

namespace InventoryManagementAPI.Interfaces
{
    public interface ITokenService
    {
        public string GenerateJwtToken(User user);
        string GenerateJwtToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken(User user);
        ClaimsPrincipal GetPrincipalFromToken(string token); 
    }
}