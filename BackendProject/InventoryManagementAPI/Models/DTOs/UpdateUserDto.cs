using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class UpdateUserDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Role ID must be a positive integer.")]
        public int RoleId { get; set; } 
    }
}
