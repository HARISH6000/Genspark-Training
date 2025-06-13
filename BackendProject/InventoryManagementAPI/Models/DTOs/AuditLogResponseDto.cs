using System;

namespace InventoryManagementAPI.DTOs
{
    public class AuditLogResponseDto
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; } 
        public DateTime Timestamp { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // "INSERT", "UPDATE", "DELETE"
        public string? OldValues { get; set; } // JSON string
        public string? NewValues { get; set; } // JSON string
        public string? Changes { get; set; } // Summary of changes
    }
}

