using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;

namespace InventoryManagementAPI.Mappers
{
    public static class InventoryManagerMapper
    {
        /// <summary>
        /// Maps an AssignRemoveInventoryManagerDto to a new InventoryManager model.
        /// </summary>
        public static InventoryManager ToInventoryManager(AssignRemoveInventoryManagerDto dto)
        {
            return new InventoryManager
            {
                InventoryId = dto.InventoryId,
                ManagerId = dto.ManagerId
            };
        }

        /// <summary>
        /// Maps an InventoryManager model to an InventoryManagerResponseDto.
        /// Requires Inventory and Manager navigation properties to be loaded.
        /// </summary>
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

        /// <summary>
        /// Maps a User model (acting as a Manager) to a ManagerForInventoryResponseDto.
        /// Requires Role navigation property to be loaded.
        /// </summary>
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

        /// <summary>
        /// Maps an Inventory model to an InventoryManagedByManagerResponseDto.
        /// </summary>
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
