namespace InventoryManagementAPI.DTOs
{
    public class LowStockNotificationDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? SKU { get; set; } = null;
        public int? CurrentQuantity { get; set; } = null;
        public int? MinStockQuantity { get; set; } = null;
        public int InventoryId { get; set; }
        public string InventoryName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
