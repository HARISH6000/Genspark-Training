using InventoryManagementAPI.Contexts;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Repositories
{
    public class AuditLogRepository : Repository<int, AuditLog>
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context)
        {

        }
        public override async Task<AuditLog?> Get(int key)
        {
            return await _applicationDbContext.AuditLogs.SingleOrDefaultAsync(al => al.AuditLogId == key);
        }

        public override async Task<IEnumerable<AuditLog>> GetAll()
        {
            return await _applicationDbContext.AuditLogs.ToListAsync();
        }
            
    }
}