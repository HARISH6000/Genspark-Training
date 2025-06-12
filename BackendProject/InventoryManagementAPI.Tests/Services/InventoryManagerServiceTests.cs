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
using System.Linq.Expressions;

namespace InventoryManagementAPI.Tests.Services
{
    public class InventoryManagerServiceTests
    {
        private readonly Mock<IInventoryManagerRepository> _mockInventoryManagerRepository;
        private readonly Mock<IInventoryRepository> _mockInventoryRepository;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly InventoryManagerService _inventoryManagerService;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public InventoryManagerServiceTests()
        {
            _mockInventoryManagerRepository = new Mock<IInventoryManagerRepository>();
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _mockUserRepository = new Mock<IUserRepository>();
            _mockAuditLogService = new Mock<IAuditLogService>();

            _inventoryManagerService = new InventoryManagerService(
                _mockInventoryManagerRepository.Object,
                _mockInventoryRepository.Object,
                _mockUserRepository.Object,
                _mockAuditLogService.Object
            );
        }

        #region AssignManagerToInventoryAsync Tests

        [Fact]
        public async Task AssignManagerToInventoryAsync_ValidAssignment_ReturnsDto()
        {
            // Arrange
            var dto = new AssignRemoveInventoryManagerDto { InventoryId = 1, ManagerId = 101 };
            var currentUserId = 1;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            var manager = new User { UserId = 101, Username = "manager1", IsDeleted = false, Role = new Role { RoleName = "Manager" } };
            var assignment = new InventoryManager { Id = 1, InventoryId = 1, ManagerId = 101, Inventory = inventory, Manager = manager };

            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync(inventory);
            _mockUserRepository.Setup(repo => repo.Get(dto.ManagerId)).ReturnsAsync(manager);
            // Changed from IsUserManagerOfInventory to GetByInventoryAndManagerId as per service logic
            _mockInventoryManagerRepository.Setup(repo => repo.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId))
                                           .ReturnsAsync((InventoryManager)null); // No existing assignment
            _mockInventoryManagerRepository.Setup(repo => repo.Add(It.IsAny<InventoryManager>())).ReturnsAsync(assignment);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryManagerService.AssignManagerToInventoryAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(assignment.Id, result.Id);
            Assert.Equal(dto.InventoryId, result.InventoryId);
            Assert.Equal(dto.ManagerId, result.ManagerId);

            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockUserRepository.Verify(repo => repo.Get(dto.ManagerId), Times.Once);
            // Verify GetByInventoryAndManagerId is called, not IsUserManagerOfInventory
            _mockInventoryManagerRepository.Verify(repo => repo.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.Add(It.Is<InventoryManager>(im =>
                im.InventoryId == dto.InventoryId && im.ManagerId == dto.ManagerId)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.UserId == currentUserId &&
                log.TableName == "InventoryManagers" &&
                log.RecordId == assignment.Id.ToString() &&
                log.ActionType == "INSERT" // Action type should be INSERT for new assignments
            )), Times.Once);
        }

        [Fact]
        public async Task AssignManagerToInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new AssignRemoveInventoryManagerDto { InventoryId = 99, ManagerId = 101 };
            var currentUserId = 1;

            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.AssignManagerToInventoryAsync(dto, currentUserId));
            Assert.Equal($"Inventory with ID {dto.InventoryId} not found or is deleted.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockUserRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockInventoryManagerRepository.Verify(repo => repo.GetByInventoryAndManagerId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockInventoryManagerRepository.Verify(repo => repo.Add(It.IsAny<InventoryManager>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task AssignManagerToInventoryAsync_ManagerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new AssignRemoveInventoryManagerDto { InventoryId = 1, ManagerId = 999 };
            var currentUserId = 1;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync(inventory);
            _mockUserRepository.Setup(repo => repo.Get(dto.ManagerId)).ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.AssignManagerToInventoryAsync(dto, currentUserId));
            Assert.Equal($"Manager (User ID {dto.ManagerId}) not found or is deleted.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockUserRepository.Verify(repo => repo.Get(dto.ManagerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetByInventoryAndManagerId(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _mockInventoryManagerRepository.Verify(repo => repo.Add(It.IsAny<InventoryManager>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region RemoveManagerFromInventoryAsync Tests

        [Fact]
        public async Task RemoveManagerFromInventoryAsync_ValidRemoval_ReturnsDto()
        {
            // Arrange
            var dto = new AssignRemoveInventoryManagerDto { InventoryId = 1, ManagerId = 101 };
            var currentUserId = 1;
            var existingAssignment = new InventoryManager { Id = 1, InventoryId = 1, ManagerId = 101, Inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" }, Manager = new User { UserId = 101, Username = "manager1" } };

            // Mock GetByInventoryAndManagerId as per service logic
            _mockInventoryManagerRepository.Setup(repo => repo.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId))
                                           .ReturnsAsync(existingAssignment);
            _mockInventoryManagerRepository.Setup(repo => repo.Delete(existingAssignment.Id)).ReturnsAsync(existingAssignment);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryManagerService.RemoveManagerFromInventoryAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingAssignment.Id, result.Id);
            Assert.Equal(dto.InventoryId, result.InventoryId);
            Assert.Equal(dto.ManagerId, result.ManagerId);

            _mockInventoryManagerRepository.Verify(repo => repo.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.Delete(existingAssignment.Id), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.UserId == currentUserId &&
                log.TableName == "InventoryManagers" &&
                log.RecordId == existingAssignment.Id.ToString() &&
                log.ActionType == "DELETE" // Action type should be DELETE for removal
            )), Times.Once);
        }

        [Fact]
        public async Task RemoveManagerFromInventoryAsync_AssignmentNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new AssignRemoveInventoryManagerDto { InventoryId = 1, ManagerId = 101 };
            var currentUserId = 1;

            // Mock GetByInventoryAndManagerId to return null
            _mockInventoryManagerRepository.Setup(repo => repo.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId))
                                           .ReturnsAsync((InventoryManager)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.RemoveManagerFromInventoryAsync(dto, currentUserId));
            // Corrected the expected exception message
            Assert.Equal($"Assignment not found for Inventory ID {dto.InventoryId} and Manager ID {dto.ManagerId}.", exception.Message);

            _mockInventoryManagerRepository.Verify(repo => repo.GetByInventoryAndManagerId(dto.InventoryId, dto.ManagerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region GetManagersForInventoryAsync Tests

        [Fact]
        public async Task GetManagersForInventoryAsync_InventoryExists_ReturnsManagers()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false };
            var managers = new List<InventoryManager>
            {
                new InventoryManager { Id = 1, InventoryId = 1, ManagerId = 101, Inventory = inventory, Manager = new User { UserId = 101, Username = "manager1", IsDeleted = false, Role = new Role { RoleName = "Manager" } } },
                new InventoryManager { Id = 2, InventoryId = 1, ManagerId = 102, Inventory = inventory, Manager = new User { UserId = 102, Username = "manager2", IsDeleted = false, Role = new Role { RoleName = "User" } } }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(inventory);
            _mockInventoryManagerRepository.Setup(repo => repo.GetManagersForInventory(inventoryId)).ReturnsAsync(managers);

            // Act
            var result = await _inventoryManagerService.GetManagersForInventoryAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, m => m.UserId == 101 && m.Username == "manager1");
            Assert.Contains(result, m => m.UserId == 102 && m.Username == "manager2");

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetManagersForInventory(inventoryId), Times.Once);
        }

        [Fact]
        public async Task GetManagersForInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.GetManagersForInventoryAsync(inventoryId));
            Assert.Equal($"Inventory with ID {inventoryId} not found or is deleted.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetManagersForInventory(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetManagersForInventoryAsync_InventoryIsDeleted_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 1;
            var deletedInventory = new Inventory { InventoryId = 1, Name = "Deleted Warehouse", IsDeleted = true };
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(deletedInventory);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.GetManagersForInventoryAsync(inventoryId));
            Assert.Equal($"Inventory with ID {inventoryId} not found or is deleted.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetManagersForInventory(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetManagersForInventoryAsync_NoManagers_ReturnsEmptyList()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(inventory);
            _mockInventoryManagerRepository.Setup(repo => repo.GetManagersForInventory(inventoryId)).ReturnsAsync(new List<InventoryManager>());

            // Act
            var result = await _inventoryManagerService.GetManagersForInventoryAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockInventoryRepository.Verify(repo => repo.Get(inventoryId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetManagersForInventory(inventoryId), Times.Once);
        }

        [Fact]
        public async Task GetManagersForInventoryAsync_WithSorting_ReturnsSortedManagers()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false };
            var managers = new List<InventoryManager>
            {
                new InventoryManager { Id = 1, InventoryId = 1, ManagerId = 102, Inventory = inventory, Manager = new User { UserId = 102, Username = "Charlie", IsDeleted = false, Role = new Role { RoleName = "User" } } },
                new InventoryManager { Id = 2, InventoryId = 1, ManagerId = 101, Inventory = inventory, Manager = new User { UserId = 101, Username = "Alice", IsDeleted = false, Role = new Role { RoleName = "Manager" } } }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(inventory);
            _mockInventoryManagerRepository.Setup(repo => repo.GetManagersForInventory(inventoryId)).ReturnsAsync(managers);

            // Act
            var result = await _inventoryManagerService.GetManagersForInventoryAsync(inventoryId, "username_asc");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Alice", result.First().Username);
            Assert.Equal("Charlie", result.Last().Username);
        }

        #endregion

        #region GetInventoriesManagedByManagerAsync Tests

        [Fact]
        public async Task GetInventoriesManagedByManagerAsync_ManagerExists_ReturnsInventories()
        {
            // Arrange
            var managerId = 101;
            var manager = new User { UserId = 101, Username = "manager1", IsDeleted = false, Role = new Role { RoleName = "Manager" } };
            var assignments = new List<InventoryManager>
            {
                new InventoryManager { Id = 1, InventoryId = 1, ManagerId = 101, Inventory = new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false }, Manager = manager },
                new InventoryManager { Id = 2, InventoryId = 2, ManagerId = 101, Inventory = new Inventory { InventoryId = 2, Name = "Warehouse B", IsDeleted = false }, Manager = manager }
            };

            _mockUserRepository.Setup(repo => repo.Get(managerId)).ReturnsAsync(manager);
            _mockInventoryManagerRepository.Setup(repo => repo.GetInventoriesManagedByManager(managerId)).ReturnsAsync(assignments);

            // Act
            var result = await _inventoryManagerService.GetInventoriesManagedByManagerAsync(managerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, i => i.InventoryId == 1 && i.Name == "Warehouse A");
            Assert.Contains(result, i => i.InventoryId == 2 && i.Name == "Warehouse B");

            _mockUserRepository.Verify(repo => repo.Get(managerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetInventoriesManagedByManager(managerId), Times.Once);
        }

        [Fact]
        public async Task GetInventoriesManagedByManagerAsync_ManagerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var managerId = 999;
            _mockUserRepository.Setup(repo => repo.Get(managerId)).ReturnsAsync((User)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.GetInventoriesManagedByManagerAsync(managerId));
            Assert.Equal($"Manager (User ID {managerId}) not found or is deleted.", exception.Message);

            _mockUserRepository.Verify(repo => repo.Get(managerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetInventoriesManagedByManager(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetInventoriesManagedByManagerAsync_ManagerIsDeleted_ThrowsNotFoundException()
        {
            // Arrange
            var managerId = 101;
            var deletedManager = new User { UserId = 101, Username = "deletedManager", IsDeleted = true, Role = new Role { RoleName = "Manager" } };
            _mockUserRepository.Setup(repo => repo.Get(managerId)).ReturnsAsync(deletedManager);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryManagerService.GetInventoriesManagedByManagerAsync(managerId));
            Assert.Equal($"Manager (User ID {managerId}) not found or is deleted.", exception.Message);

            _mockUserRepository.Verify(repo => repo.Get(managerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetInventoriesManagedByManager(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetInventoriesManagedByManagerAsync_NoInventories_ReturnsEmptyList()
        {
            // Arrange
            var managerId = 101;
            var manager = new User { UserId = 101, Username = "manager1", IsDeleted = false, Role = new Role { RoleName = "Manager" } };

            _mockUserRepository.Setup(repo => repo.Get(managerId)).ReturnsAsync(manager);
            _mockInventoryManagerRepository.Setup(repo => repo.GetInventoriesManagedByManager(managerId)).ReturnsAsync(new List<InventoryManager>());

            // Act
            var result = await _inventoryManagerService.GetInventoriesManagedByManagerAsync(managerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockUserRepository.Verify(repo => repo.Get(managerId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.GetInventoriesManagedByManager(managerId), Times.Once);
        }

        [Fact]
        public async Task GetInventoriesManagedByManagerAsync_WithSorting_ReturnsSortedInventories()
        {
            // Arrange
            var managerId = 101;
            var manager = new User { UserId = 101, Username = "manager1", IsDeleted = false, Role = new Role { RoleName = "Manager" } };
            var assignments = new List<InventoryManager>
            {
                new InventoryManager { Id = 1, InventoryId = 2, ManagerId = 101, Inventory = new Inventory { InventoryId = 2, Name = "Zeta Warehouse", IsDeleted = false }, Manager = manager },
                new InventoryManager { Id = 2, InventoryId = 1, ManagerId = 101, Inventory = new Inventory { InventoryId = 1, Name = "Alpha Warehouse", IsDeleted = false }, Manager = manager }
            };

            _mockUserRepository.Setup(repo => repo.Get(managerId)).ReturnsAsync(manager);
            _mockInventoryManagerRepository.Setup(repo => repo.GetInventoriesManagedByManager(managerId)).ReturnsAsync(assignments);

            // Act
            var result = await _inventoryManagerService.GetInventoriesManagedByManagerAsync(managerId, "inventoryname_desc");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("Zeta Warehouse", result.First().Name);
            Assert.Equal("Alpha Warehouse", result.Last().Name);
        }

        #endregion

        #region GetAllAssignmentsAsync Tests

        [Fact]
        public async Task GetAllAssignmentsAsync_ReturnsAllAssignments()
        {
            // Arrange
            var assignments = new List<InventoryManager>
            {
                new InventoryManager { Id = 1, InventoryId = 1, ManagerId = 101, Inventory = new Inventory { InventoryId = 1, Name = "Warehouse A" }, Manager = new User { UserId = 101, Username = "manager1" } },
                new InventoryManager { Id = 2, InventoryId = 2, ManagerId = 102, Inventory = new Inventory { InventoryId = 2, Name = "Warehouse B" }, Manager = new User { UserId = 102, Username = "manager2" } }
            };

            _mockInventoryManagerRepository.Setup(repo => repo.GetAll()).ReturnsAsync(assignments);

            // Act
            var result = await _inventoryManagerService.GetAllAssignmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Id == 1 && a.InventoryId == 1 && a.ManagerId == 101);
            Assert.Contains(result, a => a.Id == 2 && a.InventoryId == 2 && a.ManagerId == 102);

            _mockInventoryManagerRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllAssignmentsAsync_NoAssignments_ReturnsEmptyList()
        {
            // Arrange
            _mockInventoryManagerRepository.Setup(repo => repo.GetAll()).ReturnsAsync(new List<InventoryManager>());

            // Act
            var result = await _inventoryManagerService.GetAllAssignmentsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _mockInventoryManagerRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        #endregion
    }
}
