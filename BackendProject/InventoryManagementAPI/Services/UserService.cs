using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using InventoryManagementAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json; 

namespace InventoryManagementAPI.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRepository<int, Role> _roleRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IInventoryManagerRepository _inventoryManagerRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IFileStorageService _fileStorageService; 

        public UserService(IUserRepository userRepository,
                           IRepository<int, Role> roleRepository,
                           IPasswordHasher passwordHasher,
                           IInventoryManagerRepository inventoryManagerRepository,
                           IAuditLogService auditLogService,
                           IFileStorageService fileStorageService) 
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _inventoryManagerRepository = inventoryManagerRepository;
            _auditLogService = auditLogService;
            _fileStorageService = fileStorageService;
        }
        
        

        public async Task<UserResponseDto> RegisterUserAsync(AddUserDto userDto, int? currentUserId)
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

                // --- AUDIT LOGGING: INSERT OPERATION ---
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Users",
                    RecordId = addedUser.UserId.ToString(),
                    ActionType = "INSERT",
                    NewValues = addedUser
                });
                // --- END AUDIT LOGGING ---

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
                }
                throw;
            }
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepository.Get(userId);
            if (user == null) return null;
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = _fileStorageService.GetSasUrl(user.ProfilePictureUrl, TimeSpan.FromMinutes(60));
            }
            return UserMapper.ToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsername(username);
            if (user == null) return null;
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = _fileStorageService.GetSasUrl(user.ProfilePictureUrl, TimeSpan.FromMinutes(60));
            }
            return UserMapper.ToUserResponseDto(user);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int userId, UpdateUserDto userDto, int? currentUserId)
        {
            var existingUser = await _userRepository.Get(userId);
            if (existingUser == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            var oldUserSnapshot = JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(existingUser)); 

            var role = await _roleRepository.Get(userDto.RoleId);
            if (role == null)
            {
                throw new NotFoundException($"Role with ID {userDto.RoleId} not found for update.");
            }

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
            existingUser.RoleId = userDto.RoleId;
            existingUser.Role = role;

            var updatedUser = await _userRepository.Update(userId, existingUser);
            
            // --- AUDIT LOGGING: UPDATE OPERATION ---
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Users",
                RecordId = updatedUser.UserId.ToString(),
                ActionType = "UPDATE",
                OldValues = oldUserSnapshot,
                NewValues = updatedUser
            });
            // --- END AUDIT LOGGING ---

            return UserMapper.ToUserResponseDto(updatedUser);
        }

        public async Task<UserResponseDto?> UpdateUserByUserAsync(int userId, UpdateUserbyUserDto userDto, int? currentUserId)
        {
            var existingUser = await _userRepository.Get(userId);
            if (existingUser == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            var oldUserSnapshot = JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(existingUser)); 


            if (existingUser.Email != userDto.Email)
            {
                var userWithSameEmail = await _userRepository.GetByEmail(userDto.Email);
                if (userWithSameEmail != null && userWithSameEmail.UserId != userId)
                {
                    throw new ConflictException($"Email '{userDto.Email}' is already registered.");
                }
            }

            existingUser.Email = userDto.Email;
            existingUser.Phone = userDto.Phone;

            var updatedUser = await _userRepository.Update(userId, existingUser);
            
            // --- AUDIT LOGGING: UPDATE OPERATION ---
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Users",
                RecordId = updatedUser.UserId.ToString(),
                ActionType = "UPDATE",
                OldValues = oldUserSnapshot,
                NewValues = updatedUser
            });
            // --- END AUDIT LOGGING ---

            return UserMapper.ToUserResponseDto(updatedUser);
        }

        public async Task<UserResponseDto?> DeleteUserAsync(int userId, int? currentUserId) 
        {
            var userToDelete = await _userRepository.Get(userId);
            if (userToDelete == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            if (userToDelete.IsDeleted)
            {
                return UserMapper.ToUserResponseDto(userToDelete);
            }

            var oldUserSnapshot = JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(userToDelete));

            userToDelete.IsDeleted = true; // Soft delete
            var deletedUser = await _userRepository.Update(userId, userToDelete);
            
            // --- AUDIT LOGGING: DELETE OPERATION (Soft Delete) ---
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId, 
                TableName = "Users",
                RecordId = deletedUser.UserId.ToString(),
                ActionType = "SOFT_DELETE",
                OldValues = oldUserSnapshot,
                NewValues = deletedUser
            }); 
            // --- END AUDIT LOGGING ---

            var associatedAssignments = await _inventoryManagerRepository.GetAssignmentsByManagerId(userId);
            foreach (var assignment in associatedAssignments)
            {
                await _inventoryManagerRepository.Delete(assignment.Id); // Hard delete the assignment record
            }
            
            // If the user had a profile picture, delete the old file
            if (!string.IsNullOrEmpty(userToDelete.ProfilePictureUrl))
            {
                _fileStorageService.DeleteFile(userToDelete.ProfilePictureUrl);
            }

            return UserMapper.ToUserResponseDto(deletedUser);
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAll();
            var userResponseDtos  = users.Select(u => { if (!string.IsNullOrEmpty(u.ProfilePictureUrl))
            {
                u.ProfilePictureUrl = _fileStorageService.GetSasUrl(u.ProfilePictureUrl, TimeSpan.FromMinutes(60));
            }
            return UserMapper.ToUserResponseDto(u);
            });
            return userResponseDtos;
        }
        
        public async Task<PaginationResponse<UserResponseDto>> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null, bool includeDeleted = false)
        {

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);


            IQueryable<User> query = _userRepository.GetAllAsQueryable();

            query = query.Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(searchTerm));
            }

            int totalRecords = await query.CountAsync();


            query = query.ApplyDatabaseSorting(orderBy, "UserId");

            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var userResponseDtos  = users.Select(u => { if (!string.IsNullOrEmpty(u.ProfilePictureUrl))
            {
                u.ProfilePictureUrl = _fileStorageService.GetSasUrl(u.ProfilePictureUrl, TimeSpan.FromMinutes(60));
            }
            return UserMapper.ToUserResponseDto(u);
            });

            //var userResponseDtos = users.Select(u => UserMapper.ToUserResponseDto(u));

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var paginationMetadata = new PaginationMetadata
            {
                TotalRecords = totalRecords,
                Page = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new PaginationResponse<UserResponseDto>
            {
                Data = userResponseDtos,
                Pagination = paginationMetadata
            };
        }

        
        public async Task<UserResponseDto> UploadProfilePictureAsync(int userId, byte[] fileBytes, string fileName, string contentType, int? currentUserId)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new UnsupportedMediaTypeException("Unsupported file type.");

            var user = await _userRepository.Get(userId);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            var oldUserSnapshot = JsonSerializer.Deserialize<User>(JsonSerializer.Serialize(user));

            // Delete existing profile picture if any
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                _fileStorageService.DeleteFile(user.ProfilePictureUrl);
            }


            var newFileUrl = await _fileStorageService.SaveFileAsync(fileBytes, fileName, contentType);

            // Update the user's ProfilePictureUrl with the new file name
            user.ProfilePictureUrl = newFileUrl; // Store only the file name, not the full path

            var updatedUser = await _userRepository.Update(userId, user);

            // --- AUDIT LOGGING: UPDATE OPERATION for Profile Picture ---
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Users",
                RecordId = updatedUser.UserId.ToString(),
                ActionType = "UPDATE_PROFILE_PICTURE",
                OldValues = oldUserSnapshot,
                NewValues = updatedUser
            });
            // --- END AUDIT LOGGING ---

            return UserMapper.ToUserResponseDto(updatedUser);
        }
    }
}