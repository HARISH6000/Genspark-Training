using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions; 
using InventoryManagementAPI.Mappers; 
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<int, Role> _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IInventoryManagerRepository _inventoryManagerRepository;
        // private readonly IAuditLogService _auditLogService; // To be implemented later for audit logging

        public UserService(IUserRepository userRepository,
                           IRepository<int, Role> roleRepository,
                           IPasswordHasher passwordHasher,
                           IInventoryManagerRepository inventoryManagerRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _inventoryManagerRepository = inventoryManagerRepository;
            // _auditLogService = auditLogService;
        }

        public async Task<UserResponseDto> RegisterUserAsync(AddUserDto userDto)
        {
            
            var role = await _roleRepository.Get(userDto.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {userDto.RoleId} not found.");
            }

            
            var existingUserByUsername = await _userRepository.GetByUsername(userDto.Username);
            if (existingUserByUsername != null)
            {
                throw new ConflictException($"Username '{userDto.Username}' is already taken.");
            }

            
            var existingUserByEmail = await _userRepository.GetByEmail(userDto.Email);
            if (existingUserByEmail != null)
            {
                 throw new ConflictException($"Email '{userDto.Email}' is already registered.");
            }

            
            string hashedPassword = _passwordHasher.HashPassword(userDto.Password);

            
            var newUser = UserMapper.ToUser(userDto, hashedPassword);
            newUser.Role = role; 

            
            try
            {
                var addedUser = await _userRepository.Add(newUser);
                
                return UserMapper.ToUserResponseDto(addedUser);
            }
            catch (DbUpdateException ex) 
            {
                
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("Username") || ex.InnerException.Message.Contains("username"))
                        throw new ConflictException($"Username '{userDto.Username}' is already taken. (Database constraint)");
                    if (ex.InnerException.Message.Contains("Email") || ex.InnerException.Message.Contains("email"))
                        throw new ConflictException($"Email '{userDto.Email}' is already registered. (Database constraint)");
                    if (ex.InnerException.Message.Contains("SKU") || ex.InnerException.Message.Contains("sku"))
                        throw new ConflictException($"SKU unique constraint violation. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int userId) // Changed return type to UserResponseDto
        {
            var user = await _userRepository.Get(userId);
            if (user == null) return null;

            return UserMapper.ToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> GetUserByUsernameAsync(string username) // Changed return type to UserResponseDto
        {
            var user = await _userRepository.GetByUsername(username);
            if (user == null) return null;
            
            return UserMapper.ToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int userId, AddUserDto userDto) 
        {
            var existingUser = await _userRepository.Get(userId);
            if (existingUser == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            // Check if role exists for the update
            var role = await _roleRepository.Get(userDto.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {userDto.RoleId} not found for update.");
            }

            // Check for unique username/email if they are being changed to an already existing one
            if (existingUser.Username != userDto.Username)
            {
                var userWithSameUsername = await _userRepository.GetByUsername(userDto.Username);
                if (userWithSameUsername != null && userWithSameUsername.UserId != userId)
                {
                    throw new ConflictException($"Username '{userDto.Username}' is already taken.");
                }
            }

            if (existingUser.Email != userDto.Email)
            {
                var userWithSameEmail = await _userRepository.GetByEmail(userDto.Email);
                if (userWithSameEmail != null && userWithSameEmail.UserId != userId)
                {
                    throw new ConflictException($"Email '{userDto.Email}' is already registered.");
                }
            }


            existingUser.Username = userDto.Username;
            existingUser.Email = userDto.Email;
            existingUser.Phone = userDto.Phone;
            existingUser.ProfilePictureUrl = userDto.ProfilePictureUrl;
            existingUser.RoleId = userDto.RoleId;
            existingUser.Role = role;

            var updatedUser = await _userRepository.Update(userId, existingUser);
            // _auditLogService.LogActionAsync(currentUserId, "User", userId, "UPDATE", oldValues, updatedUser);
            return UserMapper.ToUserResponseDto(updatedUser);
        }

        public async Task<UserResponseDto?> DeleteUserAsync(int userId)
        {
            var userToDelete = await _userRepository.Get(userId);
            if (userToDelete == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            userToDelete.IsDeleted = true; // Soft delete
            var deletedUser = await _userRepository.Update(userId, userToDelete);
            // _auditLogService.LogActionAsync(currentUserId, "User", userId, "DELETE", userToDelete, null);
            return UserMapper.ToUserResponseDto(deletedUser);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync() // Changed return type to IEnumerable<UserResponseDto>
        {
            var users = await _userRepository.GetAll();

            return users.Select(u => UserMapper.ToUserResponseDto(u));
        }
    }
    
}
