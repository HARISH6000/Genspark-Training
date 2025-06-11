using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models; 
using InventoryManagementAPI.Contexts; 
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ApplicationDbContext _context;

        public TokenBlacklistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTokenToBlacklist(string jti, DateTime expiration)
        {
            await RemoveExpiredTokens();

            
            if (await _context.RevokedTokens.AnyAsync(rt => rt.Jti == jti))
            {
                throw new InvalidOperationException($"Token with JTI '{jti}' is already blacklisted.");
            }

            var revokedToken = new RevokedToken
            {
                Jti = jti,
                ExpirationDate = expiration.ToUniversalTime()
            };

            _context.RevokedTokens.Add(revokedToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTokenBlacklisted(string jti)
        {
            
            await RemoveExpiredTokens();

            
            return await _context.RevokedTokens
                                 .AnyAsync(rt => rt.Jti == jti && rt.ExpirationDate > DateTime.UtcNow);
        }

        private async Task RemoveExpiredTokens()
        {
            var expiredTokens = await _context.RevokedTokens
                                            .Where(rt => rt.ExpirationDate <= DateTime.UtcNow)
                                            .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.RevokedTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
            }
        }
    }
}