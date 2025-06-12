using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace InventoryManagementAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GenerateToken(IEnumerable<Claim> claims, int expirationMinutes)
        {
            var jwtSecret = _configuration["Jwt:Secret"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
            {
                throw new InvalidOperationException("JWT settings (Secret, Issuer, Audience) are not configured properly.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateJwtToken(User user)
        {
            var jwtExpirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserID", user.UserId.ToString()),
                new Claim("Username", user.Username),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
            };
            return GenerateToken(claims, jwtExpirationMinutes);
        }

        public string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var jwtExpirationMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");
            // Filter out Jti and exp claims to generate a new one
            var newClaims = claims.Where(c => c.Type != JwtRegisteredClaimNames.Jti && c.Type != JwtRegisteredClaimNames.Exp);
            // Add a new Jti claim for the new token
            newClaims = newClaims.Append(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            return GenerateToken(newClaims, jwtExpirationMinutes);
        }

        public string GenerateRefreshToken(User user)
        {
            var refreshTokenExpirationMinutes = _configuration.GetValue<int>("Jwt:RefreshTokenExpirationMinutes", 10080); // 7 days
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Unique ID for the refresh token
                new Claim("UserID", user.UserId.ToString()),
                new Claim("TokenType", "RefreshToken") // Custom claim to identify it as a refresh token
            };
            return GenerateToken(claims, refreshTokenExpirationMinutes);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token.");
            }

            return principal;
        }
    }
}