using System.Security.Claims; 
using System.Linq; 

namespace InventoryManagementAPI.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        
        public static int? GetUserId(this ClaimsPrincipal principal)
        {
            
            var userIdClaim = principal.FindFirst("UserID");

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        public static string? GetRole(this ClaimsPrincipal principal)
        {
            
            var roleClaim = principal.FindFirst(ClaimTypes.Role);

            if (roleClaim != null)
            {
                Console.WriteLine($"yrole: {roleClaim.Value}");
                return roleClaim.Value;
            }
            return null;
        }
    }
}
