using InventoryManagementAPI.Models;
using InventoryManagementAPI.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(UserLoginDto userLoginDto);
        Task<LoginResponseDto> RefreshToken(string refreshToken);
        Task Logout(string userId, string accessTokenString, string refreshToken);
    }
}