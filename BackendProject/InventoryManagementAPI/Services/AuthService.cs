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

namespace InventoryManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(UserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> Login(UserLoginDto userLoginDto)
        {
           
            var users = await _userRepository.GetAll();
            var user = users.FirstOrDefault(u => u.Username.Equals(userLoginDto.Username, StringComparison.OrdinalIgnoreCase));

            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (!PasswordHasher.VerifyPassword(userLoginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            if (user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User account is deactivated.");
            }

            var token = GenerateJwtToken(user);

            return new LoginResponseDto
            {
                UserId = user.UserID,
                Username = user.Username,
                Token = token
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"];
            var jwtIssuer = _configuration["JwtSettings:Issuer"];
            var jwtAudience = _configuration["JwtSettings:Audience"];

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT settings (Secret, Issuer, Audience) are not configured properly.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username), // Subject (typically username)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID
                new Claim("UserID", user.UserID.ToString()), // Custom claim for UserID
                new Claim("Username", user.Username), // Custom claim for Username
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(60), 
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
