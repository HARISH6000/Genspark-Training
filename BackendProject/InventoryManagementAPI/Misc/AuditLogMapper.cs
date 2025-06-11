using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class AuditLogMapper
    {
        
        public static AuditLogResponseDto ToAuditLogResponseDto(AuditLog auditLog)
        {
            return new AuditLogResponseDto
            {
                AuditLogId = auditLog.AuditLogId,
                UserId = auditLog.UserId,
                Username = auditLog.User?.Username,
                Timestamp = auditLog.Timestamp,
                TableName = auditLog.TableName,
                RecordId = auditLog.RecordId,
                ActionType = auditLog.ActionType,
                OldValues = auditLog.OldValues,
                NewValues = auditLog.NewValues,
                Changes = auditLog.Changes
            };
        }

        public static AuditLog ToAuditLog(AuditLogEntryDto dto)
        {
            return new AuditLog
            {
                UserId = dto.UserId,
                TableName = dto.TableName,
                RecordId = dto.RecordId,
                ActionType = dto.ActionType,
                OldValues = dto.OldValues as string, 
                NewValues = dto.NewValues as string, 
                Changes = dto.Changes,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
