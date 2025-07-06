namespace InventoryManagementAPI.DTOs
{
    public class ProductInInventoryResponseDto
    {
        public int Id{ get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int QuantityInInventory { get; set; }
        public int MinStockQuantity { get; set; }
    }
}
