using AspNetCoreRateLimit; 
using Microsoft.AspNetCore.Http; 
using System.Security.Claims; 
using System.Threading.Tasks; 
using InventoryManagementAPI.Utilities; 

namespace InventoryManagementAPI.Utilities
{
    
    public class RateLimitUserIdClientResolveContributor : IClientResolveContributor
    {

        public Task<string> ResolveClientAsync(HttpContext httpContext) 
        {
            
            var userId = httpContext?.User.GetUserId();

            // Return the user ID as the client ID.
            // If userId is null (e.g., unauthenticated user), return "anonymous"
            var clientId = userId.HasValue ? userId.Value.ToString() : "anonymous";

            return Task.FromResult(clientId);
        }
    }
}
