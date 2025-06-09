namespace InventoryManagementAPI.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int RoleID { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public Role? Role { get; set; }
        public ICollection<InventoryManager>? ManagedInventories { get; set; }
    }
}