using Xunit;
using Moq;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace InventoryManagementAPI.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryRepository> _mockInventoryRepository;
        private readonly Mock<IInventoryManagerRepository> _mockInventoryManagerRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly InventoryService _inventoryService;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public InventoryServiceTests()
        {
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _mockInventoryManagerRepository = new Mock<IInventoryManagerRepository>();
            _mockAuditLogService = new Mock<IAuditLogService>();

            _inventoryService = new InventoryService(
                _mockInventoryRepository.Object,
                _mockInventoryManagerRepository.Object,
                _mockAuditLogService.Object
            );
        }

        #region AddInventoryAsync Tests

        [Fact]
        public async Task AddInventoryAsync_ValidData_ReturnsInventoryResponseDto()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "New Warehouse", Location = "Location A" };
            var addedInventory = new Inventory { InventoryId = 1, Name = "New Warehouse", Location = "Location A", IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.GetByName(addInventoryDto.Name))
                                   .ReturnsAsync((Inventory)null);
            _mockInventoryRepository.Setup(repo => repo.Add(It.IsAny<Inventory>()))
                                   .ReturnsAsync(addedInventory);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryService.AddInventoryAsync(addInventoryDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedInventory.InventoryId, result.InventoryId);
            Assert.Equal(addInventoryDto.Name, result.Name);
            _mockInventoryRepository.Verify(repo => repo.Add(It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        [Fact]
        public async Task AddInventoryAsync_DuplicateName_ThrowsConflictException()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "Existing Warehouse", Location = "Location B" };
            var existingInventory = new Inventory { InventoryId = 1, Name = "Existing Warehouse" };

            _mockInventoryRepository.Setup(repo => repo.GetByName(addInventoryDto.Name))
                                   .ReturnsAsync(existingInventory);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryService.AddInventoryAsync(addInventoryDto, 1));
            Assert.Equal($"Inventory with name '{addInventoryDto.Name}' already exists.", exception.Message);
            _mockInventoryRepository.Verify(repo => repo.Add(It.IsAny<Inventory>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region GetInventoryByIdAsync Tests

        [Fact]
        public async Task GetInventoryByIdAsync_ExistingId_ReturnsInventoryResponseDto()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Test Warehouse", Location = "Test Location", IsDeleted = false };
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(inventory);

            // Act
            var result = await _inventoryService.GetInventoryByIdAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
        }

        [Fact]
        public async Task GetInventoryByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var inventoryId = 99;
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory)null);

            // Act
            var result = await _inventoryService.GetInventoryByIdAsync(inventoryId);

            // Assert
            Assert.Null(result); // Assert that null is returned
        }

        #endregion

        #region GetAllInventoriesAsync Tests

        [Fact]
        public async Task GetAllInventoriesAsync_ReturnsAllInventories()
        {
            // Arrange
            var inventories = new List<Inventory>
            {
                new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false },
                new Inventory { InventoryId = 2, Name = "Warehouse B", IsDeleted = false },
                new Inventory { InventoryId = 3, Name = "Deleted Warehouse", IsDeleted = true }
            };
            _mockInventoryRepository.Setup(repo => repo.GetAll()).ReturnsAsync(inventories);

            // Act
            var result = await _inventoryService.GetAllInventoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count()); // Only active inventories should be returned by default
            Assert.DoesNotContain(result, i => i.Name == "Deleted Warehouse");
        }

        [Fact]
        public async Task GetAllInventoriesAsync_IncludeDeleted_ReturnsAllInventoriesIncludingDeleted()
        {
            // Arrange
            var inventories = new List<Inventory>
            {
                new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false },
                new Inventory { InventoryId = 2, Name = "Warehouse B", IsDeleted = false },
                new Inventory { InventoryId = 3, Name = "Deleted Warehouse", IsDeleted = true }
            };
            _mockInventoryRepository.Setup(repo => repo.GetAll()).ReturnsAsync(inventories);

            // Act
            var result = await _inventoryService.GetAllInventoriesAsync(includeDeleted: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count()); // All inventories, including deleted
            Assert.Contains(result, i => i.Name == "Deleted Warehouse");
        }

        #endregion

        #region UpdateInventoryAsync Tests

        [Fact]
        public async Task UpdateInventoryAsync_ValidData_ReturnsUpdatedDto()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 1, Name = "Updated Warehouse", Location = "New Location" };
            var existingInventory = new Inventory { InventoryId = 1, Name = "Old Warehouse", Location = "Old Location", IsDeleted = false };
            var updatedInventory = new Inventory { InventoryId = 1, Name = "Updated Warehouse", Location = "New Location", IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId)).ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.GetByName(updateDto.Name)).ReturnsAsync((Inventory)null); // No conflict
            _mockInventoryRepository.Setup(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()))
                                   .ReturnsAsync(updatedInventory);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryService.UpdateInventoryAsync(updateDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Location, result.Location);
            _mockInventoryRepository.Verify(repo => repo.Update(updateDto.InventoryId, It.Is<Inventory>(i => i.Name == updateDto.Name && i.Location == updateDto.Location)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        #endregion

        #region SoftDeleteInventoryAsync Tests

        [Fact]
        public async Task SoftDeleteInventoryAsync_ValidId_SoftDeletesInventory()
        {
            // Arrange
            var inventoryId = 1;
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse to Delete", IsDeleted = false };
            var softDeletedInventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse to Delete", IsDeleted = true };
            var associatedAssignments = new List<InventoryManager> { new InventoryManager { Id = 1, InventoryId = inventoryId, ManagerId = 101 } };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Update(inventoryId, It.Is<Inventory>(i => i.IsDeleted == true)))
                                   .ReturnsAsync(softDeletedInventory);
            _mockInventoryManagerRepository.Setup(repo => repo.GetAssignmentsByInventoryId(inventoryId))
                                           .ReturnsAsync(associatedAssignments);
            _mockInventoryManagerRepository.Setup(repo => repo.Delete(It.IsAny<int>()))
                                           .ReturnsAsync(new InventoryManager());
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryService.SoftDeleteInventoryAsync(inventoryId, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
            _mockInventoryRepository.Verify(repo => repo.Update(inventoryId, It.IsAny<Inventory>()), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetAssignmentsByInventoryId(inventoryId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Exactly(associatedAssignments.Count));
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        #endregion

        #region HardDeleteInventoryAsync Tests

        [Fact]
        public async Task HardDeleteInventoryAsync_ValidId_HardDeletesInventory()
        {
            // Arrange
            var inventoryId = 1;
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse to Hard Delete", IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Delete(inventoryId)).ReturnsAsync(existingInventory);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryService.HardDeleteInventoryAsync(inventoryId, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
            _mockInventoryRepository.Verify(repo => repo.Delete(inventoryId), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        #endregion
    }
}
