using InventoryManagementAPI.Models;
using InventoryManagementAPI.Interfaces; 
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<int, RefreshToken>
    {
        Task<RefreshToken?> GetByTokenHash(string tokenHash);
        Task<IEnumerable<RefreshToken>> GetUserRefreshTokens(int userId);
        Task DeleteExpiredTokens();
        Task<RefreshToken?> GetByJti(string jti);
    }
}