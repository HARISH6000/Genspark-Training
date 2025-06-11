//Internal DTO for logging an action
namespace InventoryManagementAPI.DTOs
{
    public class AuditLogEntryDto
    {
        public int? UserId { get; set; } 
        public string TableName { get; set; } = string.Empty;
        public string RecordId { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty;
        public object? OldValues { get; set; } 
        public object? NewValues { get; set; } 
        public string? Changes { get; set; }
    }
}