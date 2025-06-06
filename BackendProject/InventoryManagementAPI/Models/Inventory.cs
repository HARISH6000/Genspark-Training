namespace InventoryManagementAPI.Models
{
    public class Inventory
    {
        public int InventoryID { get; set; }
        public string Location { get; set; } = string.Empty; 
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }

        // Navigation properties 
        public ICollection<InventoryProduct>? InventoryProducts { get; set; }
        public ICollection<InventoryManager>? InventoryManagers { get; set; }
    }
}