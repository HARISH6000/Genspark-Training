using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class AddInventoryProductDto
    {
        [Required(ErrorMessage = "Inventory ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Inventory ID must be a positive integer.")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be a positive integer.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock quantity cannot be negative.")]
        public int MinStockQuantity { get; set; } = 0;
    }
}


