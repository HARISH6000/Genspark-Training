namespace InventoryManagementAPI.DTOs
{
    public class InventoryProductResponseDto
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string InventoryName { get; set; } = string.Empty;
        public string InventoryLocation { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSKU { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStockQuantity { get; set; }
    }
}
