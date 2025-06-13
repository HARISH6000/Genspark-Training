using InventoryManagementAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Interfaces
{
    public interface IAuditLogService
    {
        
        Task<AuditLogResponseDto> LogActionAsync(AuditLogEntryDto auditLogEntry);

        Task<AuditLogResponseDto?> GetAuditLogByIdAsync(int auditLogId);

        Task<IEnumerable<AuditLogResponseDto>> GetAuditLogsAsync(AuditLogFilterDto filter, string? sortBy = null);
    }
}
