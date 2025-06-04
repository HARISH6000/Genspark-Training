using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FirstAPI.Interfaces;
using FirstAPI.Models;
using Microsoft.IdentityModel.Tokens;

namespace FirstAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _securityKey;
        public TokenService(IConfiguration configuration)
        {
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Keys:JwtTokenKey"]));
        }
        public async  Task<string> GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Username),
                new Claim(ClaimTypes.Role,user.Role)
            };
            if (user.Role == "Doctor" && user.Doctor != null)
            {
                Console.WriteLine("in....\n", user.Doctor);
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Doctor.Id.ToString()));
            }
            else if (user.Role == "Patient" && user.Patient != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Patient.Id.ToString()));
            }
            var creds = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}