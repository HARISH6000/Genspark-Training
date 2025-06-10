using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class AssignRemoveInventoryManagerDto
    {
        [Required(ErrorMessage = "Inventory ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Inventory ID must be a positive integer.")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Manager User ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Manager User ID must be a positive integer.")]
        public int ManagerId { get; set; }
    }
}