namespace InventoryManagementAPI.DTOs
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int RoleId
        { get; set; }
        public string RoleName { get; set; } = string.Empty; 
        public bool IsDeleted { get; set; }
    }
}
