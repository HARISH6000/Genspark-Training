using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.Interfaces;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Utilities;
using InventoryManagementAPI.DTOs;

namespace InventoryManagementAPI.Middleware
{
    public class RoleValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleValidationMiddleware(RequestDelegate next)
        {
            _next = next;
            
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {

            
            if (context.User.Identity?.IsAuthenticated == true)
            {
                
                var role= context.User.GetRole();

                var userId = context.User.GetUserId()??0;

                if (role != null && userId != null)
                {
                    
                    UserResponseDto user = await userService.GetUserByIdAsync(userId);
                    var actualRole = user.RoleName;

                    
                    Console.WriteLine($"[RoleValidationMiddleware] User ID: {userId}");
                    Console.WriteLine($"[RoleValidationMiddleware] Token Role: {role}");
                    Console.WriteLine($"[RoleValidationMiddleware] Actual Role: {actualRole ?? "NULL"}");

                    
                    if (!string.Equals(role, actualRole, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("[RoleValidationMiddleware] Role mismatch! Sending Unauthorized (401).");
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized: Your token's role does not match your actual role.");
                        return; 
                    }
                    else
                    {
                        Console.WriteLine("[RoleValidationMiddleware] Roles match. Continuing request.");
                        
                    }
                }
                else
                {
                    
                    Console.WriteLine("[RoleValidationMiddleware] Authenticated user but missing role or user ID claim in token.");
                    await context.Response.WriteAsync("Unauthorized: Your token's role does not match your actual role.");
                    return; 
                }
            }
            else
            {
                Console.WriteLine("[RoleValidationMiddleware] User not authenticated. Bypassing role validation.");
            }

            await _next(context);
        }
    }

}
