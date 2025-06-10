using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class AddInventoryDto
    {
        [Required(ErrorMessage = "Inventory name is required.")]
        [StringLength(100, ErrorMessage = "Inventory name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(200, ErrorMessage = "Location cannot exceed 200 characters.")]
        public string Location { get; set; } = string.Empty;
    }
}