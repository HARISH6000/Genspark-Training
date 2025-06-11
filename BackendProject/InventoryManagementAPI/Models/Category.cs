using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property
        [JsonIgnore]
        public ICollection<Product>? Products { get; set; }
    }
}
