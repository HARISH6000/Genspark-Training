namespace InventoryManagementAPI.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = string.Empty; // Stock Keeping Unit
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation property
        public ICollection<InventoryProduct>? InventoryProducts { get; set; }
    }
}