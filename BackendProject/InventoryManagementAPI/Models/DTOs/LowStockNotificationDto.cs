// DTOs/LowStockNotificationDto.cs
namespace InventoryManagementAPI.DTOs
{
    public class LowStockNotificationDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public int CurrentQuantity { get; set; }
        public int MinStockQuantity { get; set; }
        public int InventoryId { get; set; }
        public string InventoryName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
