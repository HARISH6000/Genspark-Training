namespace InventoryManagementAPI.DTOs
{
    public class ManagerForInventoryResponseDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string RoleName { get; set; } = string.Empty;
    }
}