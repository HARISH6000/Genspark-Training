using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class UserMapper
    {
        public static User ToUser(AddUserDto dto, string hashedPassword)
        {
            return new User
            {
                Username = dto.Username,
                PasswordHash = hashedPassword,
                Email = dto.Email,
                Phone = dto.Phone,
                ProfilePictureUrl = dto.ProfilePictureUrl,
                RoleId = dto.RoleId,
                IsDeleted = false
            };
        }

        public static UserResponseDto ToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                ProfilePictureUrl = user.ProfilePictureUrl,
                RoleId = user.RoleId,
                RoleName = user.Role?.RoleName ?? "Unknown",
                IsDeleted = user.IsDeleted
            };
        }
    }
}
