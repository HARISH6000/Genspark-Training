using InventoryManagementAPI.Models;
using InventoryManagementAPI.DTOs;
namespace InventoryManagementAPI.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(UserLoginDto userLoginDto);
    }
}