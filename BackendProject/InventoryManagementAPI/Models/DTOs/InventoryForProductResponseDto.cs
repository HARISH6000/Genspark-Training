namespace InventoryManagementAPI.DTOs
{
    public class InventoryForProductResponseDto
    {
        public int InventoryId { get; set; }
        public string InventoryName { get; set; } = string.Empty;
        public string InventoryLocation { get; set; } = string.Empty;
        public int QuantityInInventory { get; set; }
        public int MinStockQuantity { get; set; }
    }
}
