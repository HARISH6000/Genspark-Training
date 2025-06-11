using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class AdjustProductQuantityDto
    {
        [Required(ErrorMessage = "Inventory ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Inventory ID must be a positive integer.")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Product ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be a positive integer.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity change is required.")]
        public int QuantityChange { get; set; }
    }
}