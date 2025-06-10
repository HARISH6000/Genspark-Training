namespace InventoryManagementAPI.DTOs
{
    public class InventoryManagerResponseDto
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string InventoryName { get; set; } = string.Empty;
        public string InventoryLocation { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public string ManagerUsername { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
    }
}