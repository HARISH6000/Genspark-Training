using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class InventoryMapper
    {
        public static Inventory ToInventory(AddInventoryDto dto)
        {
            return new Inventory
            {
                Name = dto.Name,
                Location = dto.Location,
                IsDeleted = false 
            };
        }

        public static void ToInventory(UpdateInventoryDto dto, Inventory inventory)
        {
            
            inventory.Name = dto.Name;
            inventory.Location = dto.Location;
        }
        
        public static InventoryResponseDto ToInventoryResponseDto(Inventory inventory)
        {
            return new InventoryResponseDto
            {
                InventoryId = inventory.InventoryId,
                Name = inventory.Name,
                Location = inventory.Location,
                IsDeleted = inventory.IsDeleted
            };
        }
    }
}
