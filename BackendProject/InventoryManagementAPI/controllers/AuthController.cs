using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using InventoryManagementAPI.Exceptions;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization; 
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using InventoryManagementAPI.Utilities;
using Microsoft.AspNetCore.Http; // For StatusCodes

namespace InventoryManagementAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")] 
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            try
            {
                var response = await _authService.Login(userLoginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Authentication failed for user '{Username}': {Message}", userLoginDto.Username, ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during login for user '{Username}'.", userLoginDto.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during login." });
            }
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest(new { message = "Refresh token is required." });
            }

            try
            {
                var response = await _authService.RefreshToken(request.RefreshToken); 
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Token refresh failed: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during token refresh.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during token refresh." });
            }
        }


        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto request) 
        {
            try
            {
                
                var accessToken = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                _logger.LogWarning("Logout: Extracted Access Token: {AccessToken}", accessToken);

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized(new { message = "No access token provided for logout." });
                }

                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    return BadRequest(new { message = "Refresh token is required for logout." });
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtAccessToken = tokenHandler.ReadToken(accessToken) as JwtSecurityToken;

                if (jwtAccessToken == null || string.IsNullOrEmpty(jwtAccessToken.Id))
                {
                    return BadRequest(new { message = "Invalid access token format or missing JWT ID (JTI)." });
                }

                
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    return Unauthorized(new { message = "User ID not found in access token claims." });
                }

                
                await _authService.Logout(userId.Value.ToString(), accessToken, request.RefreshToken);

                _logger.LogInformation("User {Username} (ID: {UserId}) logged out. Access Token JTI: {AccessTokenJti}",
                    User.Identity?.Name, userId.Value, jwtAccessToken.Id);

                return Ok(new { message = "Successfully logged out." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Logout failed: {Message}", ex.Message);
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) 
            {
                _logger.LogWarning(ex, "Logout failed due to invalid operation: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Logout failed due to invalid arguments: {Message}", ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during logout.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during logout." });
            }
        }
    }
}