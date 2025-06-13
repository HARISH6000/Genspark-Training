using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto> RegisterUserAsync(AddUserDto userDto, int? currentUserId);
        Task<UserResponseDto?> GetUserByIdAsync(int userId);
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
        Task<UserResponseDto?> UpdateUserAsync(int userId, AddUserDto user, int? currentUserId);
        Task<UserResponseDto?> DeleteUserAsync(int userId, int? currentUserId);
        Task<IEnumerable<UserResponseDto>> GetAllUsersAsync();
        Task<UserResponseDto> UploadProfilePictureAsync(int userId, byte[] fileBytes, string fileName, string contentType, int? currentUserId);
    }
}
