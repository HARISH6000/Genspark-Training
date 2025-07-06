using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class UpdateUserbyUserDto
    {

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
        
    }
}
