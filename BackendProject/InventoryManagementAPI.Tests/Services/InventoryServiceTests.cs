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
using System.Text.Json;
using System.Text.Json.Serialization;

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
        public async Task AddInventoryAsync_NewInventory_ReturnsAddedInventoryDtoAndLogsAudit()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "New Warehouse", Location = "City Center" };
            var newInventory = new Inventory { InventoryId = 1, Name = "New Warehouse", Location = "City Center", IsDeleted = false };
            var currentUserId = 123;

            _mockInventoryRepository.Setup(repo => repo.GetByName(addInventoryDto.Name))
                                    .ReturnsAsync((Inventory?)null); // Use (Inventory?)null for nullable return
            _mockInventoryRepository.Setup(repo => repo.Add(It.IsAny<Inventory>()))
                                    .ReturnsAsync(newInventory); // Simulate successful addition
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto()); // Mock audit log

            // Act
            var result = await _inventoryService.AddInventoryAsync(addInventoryDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newInventory.InventoryId, result.InventoryId);
            Assert.Equal(newInventory.Name, result.Name);
            Assert.Equal(newInventory.Location, result.Location);

            _mockInventoryRepository.Verify(repo => repo.GetByName(addInventoryDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Add(It.Is<Inventory>(i => i.Name == addInventoryDto.Name && i.Location == addInventoryDto.Location)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == newInventory.InventoryId.ToString() &&
                dto.ActionType == "INSERT" &&
                dto.NewValues != null
            )), Times.Once);
        }

        [Fact]
        public async Task AddInventoryAsync_ExistingName_ThrowsConflictException()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "Existing Warehouse", Location = "Test Location" };
            var existingInventory = new Inventory { InventoryId = 1, Name = "Existing Warehouse", Location = "Old Location" };
            var currentUserId = 123;

            _mockInventoryRepository.Setup(repo => repo.GetByName(addInventoryDto.Name))
                                    .ReturnsAsync(existingInventory); // Simulate existing inventory

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryService.AddInventoryAsync(addInventoryDto, currentUserId));
            Assert.Equal($"Inventory with name '{addInventoryDto.Name}' already exists.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.GetByName(addInventoryDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Add(It.IsAny<Inventory>()), Times.Never); // Add should not be called
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never); // Audit should not be logged
        }

        [Theory]
        [InlineData("UNIQUE constraint failed: Inventories.Name")]
        [InlineData("duplicate key violates unique constraint \"IX_Inventories_Name\"")] // PostgreSQL specific
        public async Task AddInventoryAsync_DbUpdateExceptionDueToUniqueName_ThrowsConflictException(string innerExceptionMessage) // Removed unused 'columnName' parameter
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "New Warehouse", Location = "City Center" };
            var currentUserId = 123;

            _mockInventoryRepository.Setup(repo => repo.GetByName(addInventoryDto.Name))
                                    .ReturnsAsync((Inventory?)null);
            _mockInventoryRepository.Setup(repo => repo.Add(It.IsAny<Inventory>()))
                                    .ThrowsAsync(new DbUpdateException("Test DbUpdateException", new Exception(innerExceptionMessage)));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryService.AddInventoryAsync(addInventoryDto, currentUserId));
            Assert.Contains($"Inventory with name '{addInventoryDto.Name}' already exists. (Database constraint)", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.GetByName(addInventoryDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Add(It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task AddInventoryAsync_OtherDbUpdateException_ThrowsOriginalDbUpdateException()
        {
            // Arrange
            var addInventoryDto = new AddInventoryDto { Name = "New Warehouse", Location = "City Center" };
            var currentUserId = 123;
            var expectedException = new DbUpdateException("Some other database error", new Exception("Generic inner exception"));

            _mockInventoryRepository.Setup(repo => repo.GetByName(addInventoryDto.Name))
                                    .ReturnsAsync((Inventory?)null);
            _mockInventoryRepository.Setup(repo => repo.Add(It.IsAny<Inventory>()))
                                    .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _inventoryService.AddInventoryAsync(addInventoryDto, currentUserId));
            Assert.Same(expectedException, exception); // Should re-throw the original exception

            _mockInventoryRepository.Verify(repo => repo.GetByName(addInventoryDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Add(It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region GetInventoryByIdAsync Tests

        [Fact]
        public async Task GetInventoryByIdAsync_InventoryExists_ReturnsInventoryDto()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Test Inventory", Location = "Test Location" };
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync(inventory);

            // Act
            var result = await _inventoryService.GetInventoryByIdAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
            Assert.Equal(inventory.Name, result.Name);
            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
        }

        [Fact]
        public async Task GetInventoryByIdAsync_InventoryDoesNotExist_ReturnsNull()
        {
            // Arrange
            var inventoryId = 99;
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)null); // Use (Inventory?)null for nullable return

            // Act
            var result = await _inventoryService.GetInventoryByIdAsync(inventoryId);

            // Assert
            Assert.Null(result);
            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
        }

        #endregion

        #region GetAllInventoriesAsync Tests

        [Fact]
        public async Task GetAllInventoriesAsync_IncludeDeletedTrue_ReturnsAllInventories()
        {
            // Arrange
            var inventories = new List<Inventory>
            {
                new Inventory { InventoryId = 1, Name = "Active Inv", IsDeleted = false },
                new Inventory { InventoryId = 2, Name = "Deleted Inv", IsDeleted = true }
            };
            _mockInventoryRepository.Setup(repo => repo.GetAll())
                                    .ReturnsAsync(inventories);

            // Act
            var result = await _inventoryService.GetAllInventoriesAsync(includeDeleted: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, i => i.Name == "Active Inv");
            Assert.Contains(result, i => i.Name == "Deleted Inv");
            _mockInventoryRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllInventoriesAsync_IncludeDeletedFalse_ReturnsOnlyActiveInventories()
        {
            // Arrange
            var inventories = new List<Inventory>
            {
                new Inventory { InventoryId = 1, Name = "Active Inv", IsDeleted = false },
                new Inventory { InventoryId = 2, Name = "Deleted Inv", IsDeleted = true }
            };
            _mockInventoryRepository.Setup(repo => repo.GetAll())
                                    .ReturnsAsync(inventories);

            // Act
            var result = await _inventoryService.GetAllInventoriesAsync(includeDeleted: false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, i => i.Name == "Active Inv");
            Assert.DoesNotContain(result, i => i.Name == "Deleted Inv");
            _mockInventoryRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllInventoriesAsync_NoInventories_ReturnsEmptyList()
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
        public async Task UpdateInventoryAsync_ValidUpdate_ReturnsUpdatedInventoryDtoAndLogsAudit()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 1, Name = "Updated Warehouse", Location = "New Location" };
            // Removed CreatedDate as per error: CS0117
            var existingInventory = new Inventory { InventoryId = 1, Name = "Old Warehouse", Location = "Old Location", IsDeleted = false };
            var currentUserId = 456;

            // Simulate deserialization of snapshot
            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(existingInventory));
            // Serialize snapshots to string *before* the It.Is lambda to avoid CS0854
            var oldValuesJson = JsonSerializer.Serialize(oldInventorySnapshot);
            var newValuesJson = JsonSerializer.Serialize(existingInventory);


            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId))
                                    .ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.GetByName(updateDto.Name))
                                    .ReturnsAsync((Inventory?)null); // No other inventory with new name
            _mockInventoryRepository.Setup(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()))
                                    .ReturnsAsync(existingInventory); // Simulate successful update

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
            var result = await _inventoryService.UpdateInventoryAsync(updateDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.InventoryId, result.InventoryId);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Location, result.Location);

            _mockInventoryRepository.Verify(repo => repo.Get(updateDto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.GetByName(updateDto.Name), Times.Once); // Called because name changed
            _mockInventoryRepository.Verify(repo => repo.Update(updateDto.InventoryId, It.Is<Inventory>(i => i.Name == updateDto.Name && i.Location == updateDto.Location)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == updateDto.InventoryId.ToString() &&
                dto.ActionType == "UPDATE" &&
                JsonSerializer.Serialize(dto.OldValues) == oldValuesJson && // Compare serialized snapshots
                JsonSerializer.Serialize(dto.NewValues) == newValuesJson
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 99, Name = "NonExistent", Location = "Anywhere" };
            var currentUserId = 456;

            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId))
                                    .ReturnsAsync((Inventory?)null); // Use (Inventory?)null for nullable return

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryService.UpdateInventoryAsync(updateDto, currentUserId));
            Assert.Equal($"Inventory with ID {updateDto.InventoryId} not found.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(updateDto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateInventoryAsync_NameConflictsWithAnotherInventory_ThrowsConflictException()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 1, Name = "Conflicting Name", Location = "Updated Location" };
            var existingInventory = new Inventory { InventoryId = 1, Name = "Original Name", Location = "Original Location" };
            var conflictingInventory = new Inventory { InventoryId = 2, Name = "Conflicting Name", Location = "Another Location" };
            var currentUserId = 456;

            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId))
                                    .ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.GetByName(updateDto.Name))
                                    .ReturnsAsync(conflictingInventory); // Simulate another inventory already has this name

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryService.UpdateInventoryAsync(updateDto, currentUserId));
            Assert.Equal($"Inventory with name '{updateDto.Name}' already exists.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(updateDto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.GetByName(updateDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateInventoryAsync_NoNameChange_PerformsUpdateWithoutNameConflictCheck()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 1, Name = "Original Name", Location = "New Location" };
            // Removed CreatedDate as per error: CS0117
            var existingInventory = new Inventory { InventoryId = 1, Name = "Original Name", Location = "Old Location", IsDeleted = false };
            var currentUserId = 456;

            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(existingInventory));
            var oldValuesJson = JsonSerializer.Serialize(oldInventorySnapshot);
            var newValuesJson = JsonSerializer.Serialize(existingInventory);

            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId))
                                    .ReturnsAsync(existingInventory);
            // GetByName should not be called if name doesn't change
            _mockInventoryRepository.Setup(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()))
                                    .ReturnsAsync(existingInventory);

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
            var result = await _inventoryService.UpdateInventoryAsync(updateDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.Name, result.Name);
            Assert.Equal(updateDto.Location, result.Location);

            _mockInventoryRepository.Verify(repo => repo.Get(updateDto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.GetByName(It.IsAny<string>()), Times.Never); // Name conflict check skipped
            _mockInventoryRepository.Verify(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == updateDto.InventoryId.ToString() &&
                dto.ActionType == "UPDATE" &&
                JsonSerializer.Serialize(dto.OldValues) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues) == newValuesJson
            )), Times.Once);
        }

        [Theory]
        [InlineData("UNIQUE constraint failed: Inventories.Name")]
        [InlineData("duplicate key violates unique constraint \"IX_Inventories_Name\"")]
        public async Task UpdateInventoryAsync_DbUpdateExceptionDueToUniqueName_ThrowsConflictException(string innerExceptionMessage) // Removed unused 'columnName' parameter
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 1, Name = "Conflicting Name", Location = "Updated Location" };
            var existingInventory = new Inventory { InventoryId = 1, Name = "Original Name", Location = "Original Location" };
            var currentUserId = 456;

            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId))
                                    .ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.GetByName(updateDto.Name))
                                    .ReturnsAsync((Inventory?)null); // Initial check passes
            _mockInventoryRepository.Setup(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()))
                                    .ThrowsAsync(new DbUpdateException("Test DbUpdateException", new Exception(innerExceptionMessage)));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryService.UpdateInventoryAsync(updateDto, currentUserId));
            Assert.Contains($"Inventory with name '{updateDto.Name}' already exists. (Database constraint)", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(updateDto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.GetByName(updateDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateInventoryAsync_OtherDbUpdateException_ThrowsOriginalDbUpdateException()
        {
            // Arrange
            var updateDto = new UpdateInventoryDto { InventoryId = 1, Name = "Updated Name", Location = "Updated Location" };
            var existingInventory = new Inventory { InventoryId = 1, Name = "Original Name", Location = "Original Location" };
            var currentUserId = 456;
            var expectedException = new DbUpdateException("Some other database error during update", new Exception("Generic inner update exception"));

            _mockInventoryRepository.Setup(repo => repo.Get(updateDto.InventoryId))
                                    .ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.GetByName(updateDto.Name))
                                    .ReturnsAsync((Inventory?)null);
            _mockInventoryRepository.Setup(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()))
                                    .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _inventoryService.UpdateInventoryAsync(updateDto, currentUserId));
            Assert.Same(expectedException, exception);

            _mockInventoryRepository.Verify(repo => repo.Get(updateDto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.GetByName(updateDto.Name), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(updateDto.InventoryId, It.IsAny<Inventory>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region SoftDeleteInventoryAsync Tests

        [Fact]
        public async Task SoftDeleteInventoryAsync_InventoryExistsAndNotDeleted_SoftDeletesAndDeletesAssignmentsAndLogsAudit()
        {
            // Arrange
            var inventoryId = 1;
            // Removed CreatedDate as per error: CS0117
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "To Be Deleted", Location = "Location", IsDeleted = false };
            var currentUserId = 789;
            var associatedAssignments = new List<InventoryManager>
            {
                // Removed UserId as per error: CS0117
                new InventoryManager { Id = 10, InventoryId = inventoryId },
                new InventoryManager { Id = 11, InventoryId = inventoryId }
            };

            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(existingInventory));
            // Removed CreatedDate as per error: CS0117
            var updatedInventoryAfterSoftDelete = new Inventory { InventoryId = inventoryId, Name = "To Be Deleted", Location = "Location", IsDeleted = true };

            // Serialize snapshots to string *before* the It.Is lambda to avoid CS0854
            var oldValuesJson = JsonSerializer.Serialize(oldInventorySnapshot);
            var newValuesJson = JsonSerializer.Serialize(updatedInventoryAfterSoftDelete);

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Update(inventoryId, It.Is<Inventory>(i => i.IsDeleted == true)))
                                    .ReturnsAsync(updatedInventoryAfterSoftDelete);
            _mockInventoryManagerRepository.Setup(repo => repo.GetAssignmentsByInventoryId(inventoryId))
                                           .ReturnsAsync(associatedAssignments);
            _mockInventoryManagerRepository.Setup(repo => repo.Delete(It.IsAny<int>()))
                                           .ReturnsAsync(new InventoryManager()); // Simulate delete success
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
            var result = await _inventoryService.SoftDeleteInventoryAsync(inventoryId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
            Assert.Equal(inventoryId, result.InventoryId);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(inventoryId, It.Is<Inventory>(i => i.IsDeleted == true)), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetAssignmentsByInventoryId(inventoryId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.Delete(associatedAssignments[0].Id), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.Delete(associatedAssignments[1].Id), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == inventoryId.ToString() &&
                dto.ActionType == "SOFT_DELETE" &&
                JsonSerializer.Serialize(dto.OldValues) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues) == newValuesJson
            )), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var currentUserId = 789;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)null); // Use (Inventory?)null for nullable return

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryService.SoftDeleteInventoryAsync(inventoryId, currentUserId));
            Assert.Equal($"Inventory with ID {inventoryId} not found.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()), Times.Never);
            _mockInventoryManagerRepository.Verify(repo => repo.GetAssignmentsByInventoryId(It.IsAny<int>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task SoftDeleteInventoryAsync_InventoryAlreadyDeleted_ReturnsExistingDeletedInventoryDtoWithoutReDeleting()
        {
            // Arrange
            var inventoryId = 1;
            // Removed CreatedDate as per error: CS0117
            var alreadyDeletedInventory = new Inventory { InventoryId = inventoryId, Name = "Already Deleted", Location = "Location", IsDeleted = true };
            var currentUserId = 789;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync(alreadyDeletedInventory);

            // Act
            var result = await _inventoryService.SoftDeleteInventoryAsync(inventoryId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
            Assert.Equal(inventoryId, result.InventoryId);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Inventory>()), Times.Never); // Update should not be called
            _mockInventoryManagerRepository.Verify(repo => repo.GetAssignmentsByInventoryId(It.IsAny<int>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region HardDeleteInventoryAsync Tests

        [Fact]
        public async Task HardDeleteInventoryAsync_InventoryExists_DeletesInventoryAndLogsAudit()
        {
            // Arrange
            var inventoryId = 1;
            // Removed CreatedDate as per error: CS0117
            var existingInventory = new Inventory { InventoryId = inventoryId, Name = "To Be Hard Deleted", Location = "Location", IsDeleted = false };
            var currentUserId = 999;

            var oldInventorySnapshot = JsonSerializer.Deserialize<Inventory>(JsonSerializer.Serialize(existingInventory));
            // Serialize snapshot to string *before* the It.Is lambda to avoid CS0854
            var oldValuesJson = JsonSerializer.Serialize(oldInventorySnapshot);


            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync(existingInventory);
            _mockInventoryRepository.Setup(repo => repo.Delete(inventoryId))
                                    .ReturnsAsync(existingInventory); // Simulate successful hard delete
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
            var result = await _inventoryService.HardDeleteInventoryAsync(inventoryId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
            Assert.Equal(existingInventory.Name, result.Name);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Delete(inventoryId), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Inventories" &&
                dto.RecordId == inventoryId.ToString() &&
                dto.ActionType == "HARD_DELETE" &&
                JsonSerializer.Serialize(dto.OldValues) == oldValuesJson &&
                dto.NewValues == null // NewValues should be null for hard delete
            )), Times.Once);
        }

        [Fact]
        public async Task HardDeleteInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var currentUserId = 999;

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId))
                                    .ReturnsAsync((Inventory?)null); // Use (Inventory?)null for nullable return

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