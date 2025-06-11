using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models
{
    public class InventoryManager
    {

        public int Id { get; set; }
        public int InventoryId { get; set; }
        public int ManagerId { get; set; }

        // Navigation properties
        public Inventory? Inventory { get; set; }
        public User? Manager { get; set; }
    }
}