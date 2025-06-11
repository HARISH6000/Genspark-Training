using System.Text.Json.Serialization;

namespace InventoryManagementAPI.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;

        // Navigation property
        [JsonIgnore]
        public ICollection<User>? Users { get; set; }
    }
}