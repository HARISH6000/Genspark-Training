namespace InventoryManagementAPI.DTOs
{
    public class InventoryManagedByManagerResponseDto
    {
        public int InventoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
