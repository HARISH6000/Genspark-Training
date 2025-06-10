namespace InventoryManagementAPI.Models
{
    public class InventoryProduct
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Navigation properties
        public Inventory? Inventory { get; set; }
        public Product? Product { get; set; }
    }
}