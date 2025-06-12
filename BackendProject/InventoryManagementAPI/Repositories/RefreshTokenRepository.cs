using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Repositories
{
    public class RefreshTokenRepository : Repository<int, RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        
        public override async Task<RefreshToken?> Get(int key)
        {
            return await _applicationDbContext.RefreshTokens.FindAsync(key);
        }

        
        public override async Task<IEnumerable<RefreshToken>> GetAll()
        {
            return await _applicationDbContext.RefreshTokens.ToListAsync();
        }

        public async Task<RefreshToken?> GetByTokenHash(string tokenHash)
        {
            return await _applicationDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task<IEnumerable<RefreshToken>> GetUserRefreshTokens(int userId)
        {
            return await _applicationDbContext.RefreshTokens
                                 .Where(rt => rt.UserId == userId)
                                 .ToListAsync();
        }

        public async Task DeleteExpiredTokens()
        {
            var expiredTokens = await _applicationDbContext.RefreshTokens
                                            .Where(rt => rt.ExpiryDate <= DateTime.UtcNow)
                                            .ToListAsync();
            if (expiredTokens.Any())
            {
                _applicationDbContext.RefreshTokens.RemoveRange(expiredTokens);
                await _applicationDbContext.SaveChangesAsync();
            }
        }

        public async Task<RefreshToken?> GetByJti(string jti)
        {
            return await _applicationDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Jti == jti);
        }
    }
}