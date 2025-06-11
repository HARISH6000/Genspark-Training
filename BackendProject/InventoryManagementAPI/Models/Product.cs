using System.Text.Json.Serialization;

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
        public int CategoryId { get; set; }

        // Navigation property
        public Category? Category { get; set; }
        [JsonIgnore]
        public ICollection<InventoryProduct>? InventoryProducts { get; set; }
    }
}