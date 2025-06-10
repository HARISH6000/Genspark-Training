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
        
    }
}
