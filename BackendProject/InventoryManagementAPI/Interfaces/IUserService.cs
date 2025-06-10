using InventoryManagementAPI.Models;
using InventoryManagementAPI.DTOs;


namespace InventoryManagementAPI.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> RegisterUserAsync(AddUserDto userDto);
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
        Task<UserResponseDto?> UpdateUserAsync(int userId, AddUserDto user);
        Task<UserResponseDto?> DeleteUserAsync(int userId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
    }
}