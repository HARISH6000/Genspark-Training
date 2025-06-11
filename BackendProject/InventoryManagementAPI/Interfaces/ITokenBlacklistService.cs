using System;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task AddTokenToBlacklist(string jti, DateTime expiration);
        Task<bool> IsTokenBlacklisted(string jti);
    }
}