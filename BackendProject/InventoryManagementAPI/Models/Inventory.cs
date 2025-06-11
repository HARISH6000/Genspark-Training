using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models
{
    public class Inventory
    {
        public int InventoryId { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }

        // Navigation properties 
        [JsonIgnore]
        public ICollection<InventoryProduct>? InventoryProducts { get; set; }
        [JsonIgnore]
        public ICollection<InventoryManager>? InventoryManagers { get; set; }
    }
}