using System.ComponentModel.DataAnnotations;

namespace InventoryManagementAPI.DTOs
{
    public class UpdateInventoryProductMinStockDto
    {
        [Required(ErrorMessage = "Inventory ID is required for update.")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "Product ID is required for update.")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "New minimum stock quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock quantity cannot be negative.")]
        public int NewMinStockQuantity { get; set; }
    }
}
