using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models
{
    public class InventoryProduct
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
         public int MinStockQuantity { get; set; } = 0;

        // Navigation properties
        public Inventory? Inventory { get; set; }
        public Product? Product { get; set; }
    }
}