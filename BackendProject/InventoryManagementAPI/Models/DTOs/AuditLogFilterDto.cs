using System;

namespace InventoryManagementAPI.DTOs
{
    public class AuditLogFilterDto
    {
        public string? TableName { get; set; }
        public string? RecordId { get; set; }
        public string? ActionType { get; set; }
        public int? UserId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}