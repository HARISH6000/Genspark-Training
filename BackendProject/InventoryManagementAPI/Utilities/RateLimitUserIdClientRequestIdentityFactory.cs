using AspNetCoreRateLimit; // For IClientResolveContributor
using Microsoft.AspNetCore.Http; // For HttpContext
using System.Security.Claims; // For your custom GetUserId() extension method
using System.Threading.Tasks; // For Task
using InventoryManagementAPI.Utilities; // Your project's utilities namespace

namespace InventoryManagementAPI.Utilities
{
    // The class now implements IClientResolveContributor
    public class RateLimitUserIdClientResolveContributor : IClientResolveContributor
    {
        // No constructor needed if you only rely on the HttpContext parameter

        public Task<string> ResolveClientAsync(HttpContext httpContext) // <-- Correct method signature
        {
            // Use your custom GetUserId extension method to get the user ID
            var userId = httpContext?.User.GetUserId();

            // Return the user ID as the client ID.
            // If userId is null (e.g., unauthenticated user), return "anonymous"
            var clientId = userId.HasValue ? userId.Value.ToString() : "anonymous";

            // Wrap the string in a Task as the method is async
            return Task.FromResult(clientId);
        }
    }
}
