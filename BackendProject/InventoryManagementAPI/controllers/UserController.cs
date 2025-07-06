using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using InventoryManagementAPI.Utilities;
using Microsoft.AspNetCore.Http;

namespace InventoryManagementAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }


        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUser([FromBody] AddUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var currentUserId = User.GetUserId();
                var newUser = await _userService.RegisterUserAsync(userDto, currentUserId);
                return CreatedAtAction(nameof(GetUserById), new { userId = newUser.UserId }, newUser);
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "User registration conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User registration failed due to missing resource: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user registration.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during user registration." });
            }
        }


        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserById(int userId)
        {
            try
            {
                var currentUserId = User.GetUserId();

                var currentUser = await _userService.GetUserByIdAsync(currentUserId ?? 0);
                if (currentUser.RoleId != 1 && currentUser.UserId != userId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access details of user {UserId} without permission.", currentUserId, userId);
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to access this user's details." });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = $"User with ID {userId} not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user by ID {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpGet("by-username/{username}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var currentUserId = User.GetUserId();

                var currentUser = await _userService.GetUserByIdAsync(currentUserId ?? 0);
                var user = await _userService.GetUserByUsernameAsync(username);

                if (currentUser.RoleId != 1 && currentUser.UserId != user.UserId)
                {
                    _logger.LogWarning("User {CurrentUserId} attempted to access details of user {UserId} without permission.", currentUserId, user.UserId);
                    return StatusCode(StatusCodes.Status403Forbidden, new { message = "You do not have permission to access this user's details." });
                }

                if (user == null)
                {
                    return NotFound(new { message = $"User with username '{username}' not found." });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user by username {Username}.", username);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }


        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateUser(int userId, [FromBody] UpdateUserDto? userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var currentUserId = User.GetUserId();
                var role=User.GetRole();
                _logger.LogWarning("xrole:",role);
                var updatedUser = await _userService.UpdateUserAsync(userId, userDto, currentUserId);
                return Ok(updatedUser);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User update failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "User update conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user update.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during user update." });
            }
        }

        [HttpPut()]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateContactDetails([FromBody] UpdateUserbyUserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {

                var currentUserId = User.GetUserId();
                var userId = currentUserId??0;
                if(userId==0){
                    _logger.LogWarning("No valid userId found for the current user");
                    throw new Exception("Invalid UserId for current user.");
                }
                var updatedUser = await _userService.UpdateUserByUserAsync(userId, userDto, currentUserId);
                return Ok(updatedUser);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User update failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (ConflictException ex)
            {
                _logger.LogWarning(ex, "User update conflict: {Message}", ex.Message);
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user update.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during user update." });
            }
        }


        [HttpDelete("{userId}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {

                var currentUserId = User.GetUserId();
                var deletedUser = await _userService.DeleteUserAsync(userId, currentUserId);
                return Ok(deletedUser);
            }
            catch (NotFoundException ex)
            {
                _logger.LogWarning(ex, "User soft delete failed: {Message}", ex.Message);
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during user soft delete for ID {UserId}.", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during soft deletion." });
            }
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginationResponse<UserResponseDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? orderBy = null)
        {
            try
            {
                _logger.LogWarning($"xl1:{pageNumber},{pageSize},{searchTerm},{orderBy}.",pageNumber, pageSize, searchTerm, orderBy);
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    var users = await _userService.GetAllUsersAsync(pageNumber.Value, pageSize.Value, searchTerm, orderBy);
                    return Ok(users);
                }
                else
                {
                    var users = await _userService.GetAllUsersAsync();
                    return Ok(new PaginationResponse<UserResponseDto> { Data = users, Pagination = null });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all users.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred." });
            }
        }

        [HttpPut("profile-picture")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
            var currentUserId = User.GetUserId()??0;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                try
                {
                    var userResponse = await _userService.UploadProfilePictureAsync(
                        currentUserId,
                        fileBytes,
                        file.FileName,
                        file.ContentType,
                        currentUserId
                    );
                    return Ok(new {message="Profile picture uploaded successfully"});
                }
                catch (NotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnsupportedMediaTypeException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while uploading the profile picture.", error = ex.Message });
                }
            }
        }
    }
}
