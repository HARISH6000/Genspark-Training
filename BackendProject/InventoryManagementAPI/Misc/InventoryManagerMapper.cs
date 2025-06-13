using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class InventoryManagerMapper
    {
        
        public static InventoryManager ToInventoryManager(AssignRemoveInventoryManagerDto dto)
        {
            return new InventoryManager
            {
                InventoryId = dto.InventoryId,
                ManagerId = dto.ManagerId
            };
        }

        
        public static InventoryManagerResponseDto ToInventoryManagerResponseDto(InventoryManager inventoryManager)
        {
            return new InventoryManagerResponseDto
            {
                Id = inventoryManager.Id,
                InventoryId = inventoryManager.InventoryId,
                InventoryName = inventoryManager.Inventory?.Name ?? "Unknown Inventory",
                InventoryLocation = inventoryManager.Inventory?.Location ?? "Unknown Location",
                ManagerId = inventoryManager.ManagerId,
                ManagerUsername = inventoryManager.Manager?.Username ?? "Unknown Manager",
                ManagerEmail = inventoryManager.Manager?.Email ?? "Unknown Email"
            };
        }

        
        public static ManagerForInventoryResponseDto ToManagerForInventoryResponseDto(User manager)
        {
            return new ManagerForInventoryResponseDto
            {
                UserId = manager.UserId,
                Username = manager.Username,
                Email = manager.Email,
                Phone = manager.Phone,
                ProfilePictureUrl = manager.ProfilePictureUrl,
                RoleName = manager.Role?.RoleName ?? "Unknown Role"
            };
        }

        
        public static InventoryManagedByManagerResponseDto ToInventoryManagedByManagerResponseDto(Inventory inventory)
        {
            return new InventoryManagedByManagerResponseDto
            {
                InventoryId = inventory.InventoryId,
                Name = inventory.Name,
                Location = inventory.Location
            };
        }
    }
}
