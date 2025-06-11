using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class SetProductQuantityDto
    {
        [Required(ErrorMessage = "Inventory ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Inventory ID must be a positive integer.")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be a positive integer.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "New Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "New Quantity cannot be negative.")]
        public int NewQuantity { get; set; }
    }
}
