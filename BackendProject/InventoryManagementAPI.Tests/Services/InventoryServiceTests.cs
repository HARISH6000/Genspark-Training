// InventoryServiceTests.cs

using Xunit;
using Moq;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json; // Used for snapshotting
using System.Text.Json.Serialization; // Required for JsonIgnoreCondition

namespace InventoryManagementAPI.Tests.Services
{
    public class InventoryServiceTests
    {
        private readonly Mock<IInventoryRepository> _mockInventoryRepository;
        private readonly Mock<IInventoryManagerRepository> _mockInventoryManagerRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly InventoryService _inventoryService;

        // Define JsonSerializerOptions once to avoid CS0854 errors in expression trees
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
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
        public async Task AddInventoryAsync_NewInventory_ReturnsAddedInventory()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "Test Inventory", Location = "Warehouse A" };
            var newInventory = new Inventory { InventoryId = 1, Name = "Test Inventory", Location = "Warehouse A", IsDeleted = false };
            var currentUserId = 1;

            _mockInventoryRepository.Setup(repo => repo.Add(It.IsAny<Inventory>()))
                                    .ReturnsAsync((Inventory?)newInventory);

            // Act
            var result = await _inventoryService.AddInventoryAsync(addInventoryDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newInventory.InventoryId, result.InventoryId);
            Assert.Equal(addInventoryDto.Name, result.Name);
            Assert.Equal(addInventoryDto.Location, result.Location);

            _mockInventoryRepository.Verify(repo => repo.Add(It.Is<Inventory>(inv =>
                inv.Name == addInventoryDto.Name &&
                inv.Location == addInventoryDto.Location
            )), Times.Once);

            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == newInventory.InventoryId.ToString() &&
                dto.ActionType == "INSERT" &&
                dto.OldValues == null &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == JsonSerializer.Serialize(newInventory, _jsonSerializerOptions)
            )), Times.Once);
        }

        [Fact]
        public async Task AddInventoryAsync_RepositoryFails_ThrowsException()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "Fail Inventory", Location = "Warehouse B" };
            var currentUserId = 1;

            _mockInventoryRepository.Setup(repo => repo.Add(It.IsAny<Inventory>()))
                                    .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _inventoryService.AddInventoryAsync(addInventoryDto, currentUserId));
            Assert.Equal("Database error", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Add(It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region GetInventoryByIdAsync Tests

        [Fact]
        public async Task GetInventoryByIdAsync_ValidId_ReturnsInventory()
        {
            // Arrange
            var inventoryId = 1;
            var mockInventory = new Inventory { InventoryId = inventoryId, Name = "Existing Inventory", Location = "Warehouse X" };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)mockInventory);

            // Act
            var result = await _inventoryService.GetInventoryByIdAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
            Assert.Equal(mockInventory.Name, result.Name);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
        }

        [Fact]
        public async Task GetInventoryByIdAsync_InvalidId_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .Returns(Task.FromResult<Inventory?>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryService.GetInventoryByIdAsync(inventoryId));
            Assert.Equal($"Inventory with ID {inventoryId} not found.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
        }

        #endregion

        #region GetAllInventoriesAsync Tests

        [Fact]
        public async Task GetAllInventoriesAsync_ReturnsAllInventories()
        {
            // Arrange
            var mockInventories = new List<Inventory>
            {
                new Inventory { InventoryId = 1, Name = "Inv 1", Location = "Loc 1" },
                new Inventory { InventoryId = 2, Name = "Inv 2", Location = "Loc 2" }
            };

            _mockInventoryRepository.Setup(repo => repo.GetAll())
                                    .ReturnsAsync(mockInventories);

            // Act
            var result = await _inventoryService.GetAllInventoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, i => i.InventoryId == 1);
            Assert.Contains(result, i => i.InventoryId == 2);

            _mockInventoryRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllInventoriesAsync_NoInventoriesExist_ReturnsEmptyList()
        {
            // Arrange
            _mockInventoryRepository.Setup(repo => repo.GetAll())
                                    .ReturnsAsync(new List<Inventory>());

            // Act
            var result = await _inventoryService.GetAllInventoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockInventoryRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        #endregion

        #region UpdateInventoryAsync Tests

        [Fact]
        public async Task UpdateInventoryAsync_ExistingInventory_ReturnsUpdatedInventory()
        {
            // Arrange
            var inventoryId = 1;
            var updateInventoryDto = new UpdateInventoryDto { InventoryId = inventoryId, Name = "Updated Name", Location = "Updated Location" };
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "Original Name", Location = "Original Location", IsDeleted = false };
            var updatedInventory = new Inventory { InventoryId = inventoryId, Name = "Updated Name", Location = "Updated Location", IsDeleted = false };
            var currentUserId = 1;

            var oldValuesJson = JsonSerializer.Serialize(InventoryMapper.ToInventoryResponseDto(existingInventory), _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(InventoryMapper.ToInventoryResponseDto(updatedInventory), _jsonSerializerOptions);

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()))
                                    .ReturnsAsync((Inventory?)updatedInventory);

            _mockAuditLogService.Setup(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == updatedInventory.InventoryId.ToString() &&
                dto.ActionType == "UPDATE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == newValuesJson
            )))
            .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryService.UpdateInventoryAsync(updateInventoryDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateInventoryDto.Name, result.Name);
            Assert.Equal(updateInventoryDto.Location, result.Location);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(inventoryId, It.Is<Inventory>(inv =>
                inv.Name == updateInventoryDto.Name &&
                inv.Location == updateInventoryDto.Location
            )), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var updateInventoryDto = new UpdateInventoryDto { InventoryId = inventoryId, Name = "NonExistent", Location = "Somewhere" };
            var currentUserId = 1;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryService.UpdateInventoryAsync(updateInventoryDto, currentUserId));
            Assert.Equal($"Inventory with ID {inventoryId} not found.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region SoftDeleteInventoryAsync Tests

        [Fact]
        public async Task SoftDeleteInventoryAsync_ExistingInventory_ReturnsSoftDeletedInventoryDto()
        {
            // Arrange
            var inventoryId = 1;
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "Test Delete", Location = "Loc", IsDeleted = false };
            var softDeletedInventory = new Inventory { InventoryId = inventoryId, Name = "Test Delete", Location = "Loc", IsDeleted = true };
            var currentUserId = 1;

            var oldValuesJson = JsonSerializer.Serialize(InventoryMapper.ToInventoryResponseDto(existingInventory), _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(InventoryMapper.ToInventoryResponseDto(softDeletedInventory), _jsonSerializerOptions);

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()))
                                    .ReturnsAsync((Inventory?)softDeletedInventory);

            _mockAuditLogService.Setup(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == inventoryId.ToString() &&
                dto.ActionType == "SOFT_DELETE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == newValuesJson
            )))
            .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryService.SoftDeleteInventoryAsync(inventoryId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(inventoryId, It.Is<Inventory>(inv => inv.IsDeleted == true)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var currentUserId = 1;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryService.SoftDeleteInventoryAsync(inventoryId, currentUserId));
            Assert.Equal($"Inventory with ID {inventoryId} not found.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        // Removed RestoreInventoryAsync Tests as the method is not present in InventoryService.cs

        #region HardDeleteInventoryAsync Tests

        [Fact]
        public async Task HardDeleteInventoryAsync_ExistingInventory_DeletesInventoryAndLogs()
        {
            // Arrange
            var inventoryId = 1;
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "Test Hard Delete", Location = "Loc", IsDeleted = false };
            var currentUserId = 1;

            var oldValuesJson = JsonSerializer.Serialize(InventoryMapper.ToInventoryResponseDto(existingInventory), _jsonSerializerOptions);

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Delete(inventoryId))
                                    .ReturnsAsync((Inventory?)existingInventory);

            _mockAuditLogService.Setup(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == inventoryId.ToString() &&
                dto.ActionType == "HARD_DELETE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                dto.NewValues == null
            )));

            // Act
            await _inventoryService.HardDeleteInventoryAsync(inventoryId, currentUserId);

            // Assert
            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Delete(inventoryId), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        [Fact]
        public async Task HardDeleteInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var currentUserId = 999;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryService.HardDeleteInventoryAsync(inventoryId, currentUserId));
            Assert.Equal($"Inventory with ID {inventoryId} not found.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion
    }
}