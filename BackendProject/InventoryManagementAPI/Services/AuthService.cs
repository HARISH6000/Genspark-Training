using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Repositories; // Assuming IUserRepository is here
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using System.Linq;
using System.Collections.Generic; 

namespace InventoryManagementAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IRefreshTokenRepository _refreshTokenRepository; 

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ITokenBlacklistService tokenBlacklistService,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _tokenBlacklistService = tokenBlacklistService;
            _refreshTokenRepository = refreshTokenRepository;
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

            var accessToken = _tokenService.GenerateJwtToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken(user);

            // Extract JTI and expiry from the generated refresh token
            var principalForRefresh = _tokenService.GetPrincipalFromToken(refreshTokenString);
            var refreshJti = principalForRefresh.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var refreshExpiryClaim = principalForRefresh.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(refreshJti) || !long.TryParse(refreshExpiryClaim, out long unixExpiry))
            {
                throw new InvalidOperationException("Failed to extract JTI or expiry from refresh token.");
            }

            var refreshTokenExpiryDate = DateTimeOffset.FromUnixTimeSeconds(unixExpiry).UtcDateTime;

            
            var newRefreshTokenEntry = new RefreshToken
            {
                TokenHash = _passwordHasher.HashPassword(refreshTokenString), // Hashing the refresh token
                UserId = user.UserId,
                ExpiryDate = refreshTokenExpiryDate,
                IsRevoked = false,
                Jti = refreshJti // Storing JTI for potential blacklisting if needed
            };

            await _refreshTokenRepository.Add(newRefreshTokenEntry); 

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = accessToken,
                RefreshToken = refreshTokenString
            };
        }

        public async Task<LoginResponseDto> RefreshToken(string refreshTokenString)
        {
            var principal = _tokenService.GetPrincipalFromToken(refreshTokenString);

            var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user ID in refresh token claims.");
            }

            var jtiClaim = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(jtiClaim))
            {
                throw new UnauthorizedAccessException("Refresh token does not contain a JTI claim.");
            }
            
            var tokenTypeClaim = principal.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
            if (tokenTypeClaim != "RefreshToken")
            {
                throw new UnauthorizedAccessException("Provided token is not a refresh token.");
            }

            var user = await _userRepository.Get(userId);
            if (user == null || user.IsDeleted)
            {
                throw new UnauthorizedAccessException("User associated with refresh token not found or deactivated.");
            }

            var refreshTokens = await _refreshTokenRepository.GetUserRefreshTokens(userId);
            var storedRefreshToken = refreshTokens.FirstOrDefault(rt => _passwordHasher.VerifyPassword(refreshTokenString, rt.TokenHash));


            if (storedRefreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token.");
            }

            // Check if refresh token is expired
            if (storedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                storedRefreshToken.IsRevoked = true;
                await _refreshTokenRepository.Delete(storedRefreshToken.Id);
                throw new UnauthorizedAccessException("Refresh token has expired.");
            }

            // Check if refresh token is revoked
            if (storedRefreshToken.IsRevoked)
            {
                throw new UnauthorizedAccessException("Refresh token has been revoked.");
            }

            // Generate a new access token
            var newAccessToken = _tokenService.GenerateJwtToken(user);

            // Implement Refresh Token Rotation: Invalidate the old refresh token and generate a new one
            storedRefreshToken.IsRevoked = true;
            await _refreshTokenRepository.Update(storedRefreshToken.Id,storedRefreshToken); 

            var newRefreshTokenString = _tokenService.GenerateRefreshToken(user);

            // Extract JTI and expiry for the new refresh token
            var newPrincipalForRefresh = _tokenService.GetPrincipalFromToken(newRefreshTokenString);
            var newRefreshJti = newPrincipalForRefresh.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var newRefreshExpiryClaim = newPrincipalForRefresh.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(newRefreshJti) || !long.TryParse(newRefreshExpiryClaim, out long newUnixExpiry))
            {
                throw new InvalidOperationException("Failed to extract JTI or expiry from new refresh token.");
            }
            var newRefreshTokenExpiryDate = DateTimeOffset.FromUnixTimeSeconds(newUnixExpiry).UtcDateTime;

            // Create and store the new RefreshToken entry
            var newRefreshTokenEntry = new RefreshToken
            {
                TokenHash = _passwordHasher.HashPassword(newRefreshTokenString),
                UserId = user.UserId,
                ExpiryDate = newRefreshTokenExpiryDate,
                IsRevoked = false,
                Jti = newRefreshJti
            };
            await _refreshTokenRepository.Add(newRefreshTokenEntry); 

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = newAccessToken,
                RefreshToken = newRefreshTokenString
            };
        }

        
        public async Task Logout(string userIdString, string accessTokenString, string refreshTokenString)
        {
            if (!int.TryParse(userIdString, out int userId))
            {
                throw new ArgumentException("Invalid user ID format.");
            }

            if (string.IsNullOrEmpty(accessTokenString))
            {
                throw new ArgumentNullException(nameof(accessTokenString), "Access token string cannot be null or empty.");
            }


            // Blacklist the access token's JTI
            var accessTokenPrincipal = _tokenService.GetPrincipalFromToken(accessTokenString);
            var accessTokenJtiClaim = accessTokenPrincipal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var accessTokenExpClaim = accessTokenPrincipal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
            
            if (string.IsNullOrEmpty(accessTokenJtiClaim) || string.IsNullOrEmpty(accessTokenExpClaim))
            {
                throw new ArgumentException("Access token is invalid or missing JTI/Expiration.");
            }

            if (long.TryParse(accessTokenExpClaim, out long accessTokenUnixExpiry))
            {
                var accessTokenExpiration = DateTimeOffset.FromUnixTimeSeconds(accessTokenUnixExpiry).UtcDateTime;
                await _tokenBlacklistService.AddTokenToBlacklist(accessTokenJtiClaim, accessTokenExpiration);
            }
            else
            {
                throw new ArgumentException("Access token expiration claim is invalid.");
            }

            // Revoke the refresh token
            var user = await _userRepository.Get(userId);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Find the refresh token in the database
            var refreshTokens = await _refreshTokenRepository.GetUserRefreshTokens(userId);
            var storedRefreshToken = refreshTokens.FirstOrDefault(rt => _passwordHasher.VerifyPassword(refreshTokenString, rt.TokenHash));

            if (storedRefreshToken == null)
            {
                throw new UnauthorizedAccessException("Invalid refresh token for logout.");
            }

            storedRefreshToken.IsRevoked = true;
            await _refreshTokenRepository.Update(storedRefreshToken.Id,storedRefreshToken);
        }
    }
}