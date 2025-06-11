using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Linq;

namespace InventoryManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private IPasswordHasher _passwordHasher;

        private ITokenService _tokenService;


        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<LoginResponseDto> Login(UserLoginDto userLoginDto)
        {

            var users = await _userRepository.GetAll();
            var user = users.FirstOrDefault(u => u.Username.Equals(userLoginDto.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (!_passwordHasher.VerifyPassword(userLoginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User account is deactivated.");
            }

            var token = _tokenService.GenerateJwtToken(user);

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = token
            };
        }

        public async Task<LoginResponseDto> RefreshToken(ClaimsPrincipal principal)
        {

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in token claims.");
            }

            
            var user = await _userRepository.Get(userId);
            if (user == null || user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User associated with token not found or deactivated.");
            }

            var newAccessToken = _tokenService.GenerateJwtToken(principal.Claims);

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = newAccessToken
            };
        }
        
    }
}
