// InventoryManagementAPI.Tests/Services/InventoryProductServiceTests.cs
using Xunit;
using Moq;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using InventoryManagementAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq.Expressions; // Required for It.IsAny<Expression<...>> in older Moq versions, but not directly used for Get method here.

namespace InventoryManagementAPI.Tests.Services
{
    public class InventoryProductServiceTests
    {
        private readonly Mock<IInventoryProductRepository> _mockInventoryProductRepository;
        private readonly Mock<IInventoryRepository> _mockInventoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly Mock<IHubContext<LowStockHub>> _mockHubContext;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<IInventoryManagerRepository> _mockInventoryManagerRepository;
        private readonly Mock<ILogger<InventoryProductService>> _mockLogger;
        private readonly InventoryProductService _inventoryProductService;

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public InventoryProductServiceTests()
        {
            _mockInventoryProductRepository = new Mock<IInventoryProductRepository>();
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _mockHubContext = new Mock<IHubContext<LowStockHub>>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockInventoryManagerRepository = new Mock<IInventoryManagerRepository>();
            _mockLogger = new Mock<ILogger<InventoryProductService>>();

            // Setup mock for SignalR HubContext
            _mockHubContext.Setup(x => x.Clients).Returns(Mock.Of<IHubClients>());
            _mockHubContext.Setup(x => x.Clients.All).Returns(_mockClientProxy.Object);

            _inventoryProductService = new InventoryProductService(
                _mockInventoryProductRepository.Object,
                _mockInventoryRepository.Object,
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockAuditLogService.Object,
                _mockHubContext.Object,
                _mockInventoryManagerRepository.Object,
                _mockLogger.Object
            );
        }

        // Helper method to setup manager access (mocking CheckInventoryManagerAccess)
        private void SetupManagerAccess(bool isManager, int inventoryId, int? userId, Inventory? inventory = null)
        {
            _mockInventoryManagerRepository.Setup(repo => repo.IsUserManagerOfInventory(userId!.Value, inventoryId))
                                           .ReturnsAsync(isManager);

            if (!isManager && inventory != null)
            {
                _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(inventory);
            }
            else if (!isManager && inventory == null)
            {
                _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)null);
            }
        }


        #region AddInventoryProductAsync Tests

        [Fact]
        public async Task AddInventoryProductAsync_ValidDto_ReturnsAddedDtoAndLogs()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var currentUserId = 101;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            var product = new Product { ProductId = 1, SKU = "SKU001", ProductName = "Item A", IsDeleted = false, Category = new Category() };
            var newEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5, Inventory = inventory, Product = product };

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(dto.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)null);
            _mockInventoryProductRepository.Setup(repo => repo.Add(It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)newEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.AddInventoryProductAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newEntry.Id, result.Id);
            Assert.Equal(dto.Quantity, result.Quantity);
            _mockInventoryManagerRepository.Verify(repo => repo.IsUserManagerOfInventory(currentUserId, dto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Get(dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.Is<InventoryProduct>(ip => ip.InventoryId == dto.InventoryId && ip.ProductId == dto.ProductId)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "INSERT" && JsonSerializer.Serialize(log.NewValues, _jsonSerializerOptions) == JsonSerializer.Serialize(newEntry, _jsonSerializerOptions)
            )), Times.Once);
            _mockClientProxy.Verify(x => x.SendCoreAsync("ReceiveLowStockNotification", It.IsAny<object[]>(), CancellationToken.None), Times.Never); // Not low stock
        }

        [Fact]
        public async Task AddInventoryProductAsync_ForbiddenAccess_ThrowsForbiddenException()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var currentUserId = 101;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };

            SetupManagerAccess(false, dto.InventoryId, currentUserId, inventory);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ForbiddenException>(() => _inventoryProductService.AddInventoryProductAsync(dto, currentUserId));
            Assert.Equal($"User is not authorized to manage inventory '{inventory.Name}'.", exception.Message);

            _mockInventoryManagerRepository.Verify(repo => repo.IsUserManagerOfInventory(currentUserId, dto.InventoryId), Times.Once);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"User {currentUserId} attempted forbidden operation on inventory '{inventory.Name}'.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Never);
        }


        [Fact]
        public async Task AddInventoryProductAsync_NegativeQuantity_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = -5, MinStockQuantity = 0 };
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _inventoryProductService.AddInventoryProductAsync(dto, currentUserId));
            Assert.Equal("Quantity cannot be negative.", exception.Message);

            _mockInventoryManagerRepository.Verify(repo => repo.IsUserManagerOfInventory(currentUserId, dto.InventoryId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockProductRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Never);
        }

        [Fact]
        public async Task AddInventoryProductAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 99, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.AddInventoryProductAsync(dto, currentUserId));
            Assert.Equal($"Inventory with ID {dto.InventoryId} not found or is deleted.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Never);
        }

        [Fact]
        public async Task AddInventoryProductAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 99, Quantity = 10, MinStockQuantity = 5 };
            var currentUserId = 101;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(dto.ProductId)).ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.AddInventoryProductAsync(dto, currentUserId));
            Assert.Equal($"Product with ID {dto.ProductId} not found or is deleted.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Get(dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Never);
        }

        [Fact]
        public async Task AddInventoryProductAsync_ExistingAssociation_ThrowsConflictException()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var currentUserId = 101;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            var product = new Product { ProductId = 1, SKU = "SKU001", ProductName = "Item A", IsDeleted = false, Category = new Category() };
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1 };

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(dto.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryProductService.AddInventoryProductAsync(dto, currentUserId));
            Assert.Equal($"Product '{product.ProductName}' (SKU: {product.SKU}) is already associated with inventory '{inventory.Name}'. Consider updating its quantity instead.", exception.Message);

            _mockInventoryRepository.Verify(repo => repo.Get(dto.InventoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Get(dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Never);
        }

        [Theory]
        [InlineData("UNIQUE constraint failed")]
        [InlineData("duplicate key")]
        public async Task AddInventoryProductAsync_DbUpdateExceptionDueToUniqueConstraint_ThrowsConflictException(string innerExceptionMessage)
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var currentUserId = 101;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            var product = new Product { ProductId = 1, SKU = "SKU001", ProductName = "Item A", IsDeleted = false, Category = new Category() };

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(dto.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)null);
            _mockInventoryProductRepository.Setup(repo => repo.Add(It.IsAny<InventoryProduct>()))
                                           .ThrowsAsync(new DbUpdateException("Simulated unique constraint failure", new Exception(innerExceptionMessage)));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _inventoryProductService.AddInventoryProductAsync(dto, currentUserId));
            Assert.Equal($"Product '{product.ProductName}' is already assigned to inventory '{inventory.Name}'. (Database constraint)", exception.Message);

            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Once);
        }

        [Fact]
        public async Task AddInventoryProductAsync_ProductLowStock_SendsNotification()
        {
            // Arrange
            var dto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 3, MinStockQuantity = 5 }; // Quantity below min stock
            var currentUserId = 101;
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            var product = new Product { ProductId = 1, SKU = "SKU001", ProductName = "Item A", IsDeleted = false, Category = new Category() };
            var newEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 3, MinStockQuantity = 5, Inventory = inventory, Product = product };

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryRepository.Setup(repo => repo.Get(dto.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(dto.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)null);
            _mockInventoryProductRepository.Setup(repo => repo.Add(It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)newEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.AddInventoryProductAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            _mockClientProxy.Verify(x => x.SendCoreAsync(
                "ReceiveLowStockNotification",
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    (args[0] as LowStockNotificationDto)!.CurrentQuantity == 3
                ), CancellationToken.None), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "LOW_STOCK_ALERT"
            )), Times.Once);
        }

        #endregion

        #region IncreaseProductQuantityAsync Tests

        [Fact]
        public async Task IncreaseProductQuantityAsync_ValidDto_IncreasesQuantityAndLogs()
        {
            // Arrange
            var dto = new AdjustProductQuantityDto { InventoryId = 1, ProductId = 1, QuantityChange = 5 };
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" };
            var product = new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 5, Inventory = inventory, Product = product };

            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(existingEntry));
            var oldValuesJson = JsonSerializer.Serialize(oldEntrySnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(updatedEntry, _jsonSerializerOptions);

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.Update(existingEntry.Id, It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.IncreaseProductQuantityAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.Quantity);
            _mockInventoryProductRepository.Verify(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Update(existingEntry.Id, It.Is<InventoryProduct>(ip => ip.Quantity == 15)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "QUANTITY_INCREASE" &&
                JsonSerializer.Serialize(log.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(log.NewValues, _jsonSerializerOptions) == newValuesJson &&
                log.Changes!.Contains("Increased quantity by 5.")
            )), Times.Once);
            _mockClientProxy.Verify(x => x.SendCoreAsync("ReceiveLowStockNotification", It.IsAny<object[]>(), CancellationToken.None), Times.Never); // Not low stock
        }

        [Fact]
        public async Task IncreaseProductQuantityAsync_NegativeOrZeroQuantityChange_ThrowsArgumentException()
        {
            // Arrange
            var dto = new AdjustProductQuantityDto { InventoryId = 1, ProductId = 1, QuantityChange = 0 }; // Zero change
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _inventoryProductService.IncreaseProductQuantityAsync(dto, currentUserId));
            Assert.Equal("QuantityChange must be positive for increasing stock.", exception.Message);
        }

        [Fact]
        public async Task IncreaseProductQuantityAsync_EntryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new AdjustProductQuantityDto { InventoryId = 1, ProductId = 1, QuantityChange = 5 };
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.IncreaseProductQuantityAsync(dto, currentUserId));
            Assert.Equal($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}.", exception.Message);
        }

        #endregion

        #region DecreaseProductQuantityAsync Tests

        [Fact]
        public async Task DecreaseProductQuantityAsync_ValidDto_DecreasesQuantityAndLogs()
        {
            // Arrange
            var dto = new AdjustProductQuantityDto { InventoryId = 1, ProductId = 1, QuantityChange = 5 };
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" };
            var product = new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 5, MinStockQuantity = 5, Inventory = inventory, Product = product };


            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(existingEntry));
            var oldValuesJson = JsonSerializer.Serialize(oldEntrySnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(updatedEntry, _jsonSerializerOptions);

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.Update(existingEntry.Id, It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.DecreaseProductQuantityAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Quantity);
            _mockInventoryProductRepository.Verify(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Update(existingEntry.Id, It.Is<InventoryProduct>(ip => ip.Quantity == 5)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "QUANTITY_DECREASE" &&
                JsonSerializer.Serialize(log.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(log.NewValues, _jsonSerializerOptions) == newValuesJson &&
                log.Changes!.Contains("Decreased quantity by 5.")
            )), Times.Once);
            //_mockClientProxy.Verify(x => x.SendCoreAsync("ReceiveLowStockNotification", It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task DecreaseProductQuantityAsync_DecreaseBelowZero_ThrowsInvalidOperationException()
        {
            // Arrange
            var dto = new AdjustProductQuantityDto { InventoryId = 1, ProductId = 1, QuantityChange = 15 };
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)new Inventory { InventoryId = 1, Name = "Main Warehouse" });
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" });


            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _inventoryProductService.DecreaseProductQuantityAsync(dto, currentUserId));
            Assert.Equal($"Cannot decrease quantity below zero. Current stock: {existingEntry.Quantity}, requested decrease: {dto.QuantityChange}.", exception.Message);

            _mockInventoryProductRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<InventoryProduct>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region SetProductQuantityAsync Tests

        [Fact]
        public async Task SetProductQuantityAsync_ValidDto_SetsQuantityAndLogs()
        {
            // Arrange
            var dto = new SetProductQuantityDto { InventoryId = 1, ProductId = 1, NewQuantity = 20 };
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" };
            var product = new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 20, MinStockQuantity = 5, Inventory = inventory, Product = product };

            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(existingEntry));
            var oldValuesJson = JsonSerializer.Serialize(oldEntrySnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(updatedEntry, _jsonSerializerOptions);

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.Update(existingEntry.Id, It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.SetProductQuantityAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.Quantity);
            _mockInventoryProductRepository.Verify(repo => repo.Update(existingEntry.Id, It.Is<InventoryProduct>(ip => ip.Quantity == 20)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "QUANTITY_SET" &&
                JsonSerializer.Serialize(log.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(log.NewValues, _jsonSerializerOptions) == newValuesJson &&
                log.Changes!.Contains("Set quantity. From 10 to 20.")
            )), Times.Once);
        }

        [Fact]
        public async Task SetProductQuantityAsync_NegativeQuantity_ThrowsArgumentException()
        {
            // Arrange
            var dto = new SetProductQuantityDto { InventoryId = 1, ProductId = 1, NewQuantity = -5 };
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _inventoryProductService.SetProductQuantityAsync(dto, currentUserId));
            Assert.Equal("NewQuantity cannot be negative.", exception.Message);
        }

        [Fact]
        public async Task SetProductQuantityAsync_EntryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new SetProductQuantityDto { InventoryId = 1, ProductId = 1, NewQuantity = 10 };
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.SetProductQuantityAsync(dto, currentUserId));
            Assert.Equal($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}. Cannot set quantity.", exception.Message);
        }

        #endregion

        #region DeleteInventoryProductAsync Tests

        [Fact]
        public async Task DeleteInventoryProductAsync_ValidId_DeletesAndLogs()
        {
            // Arrange
            var inventoryProductId = 1;
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = inventoryProductId, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" };
            var product = new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" };
            existingEntry.Inventory = inventory;
            existingEntry.Product = product;


            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(existingEntry));
            var oldValuesJson = JsonSerializer.Serialize(oldEntrySnapshot, _jsonSerializerOptions);

            SetupManagerAccess(true, existingEntry.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.Delete(inventoryProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryProductService.DeleteInventoryProductAsync(inventoryProductId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryProductId, result.Id);
            _mockInventoryProductRepository.Verify(repo => repo.Get(inventoryProductId), Times.Once);
            _mockInventoryRepository.Verify(repo => repo.Get(existingEntry.InventoryId), Times.Once); // For CheckInventoryManagerAccess
            _mockInventoryProductRepository.Verify(repo => repo.Delete(inventoryProductId), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "DELETE" &&
                JsonSerializer.Serialize(log.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                log.NewValues == null
            )), Times.Once);
        }

        [Fact]
        public async Task DeleteInventoryProductAsync_EntryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryProductId = 99;
            var currentUserId = 101;
            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId)).ReturnsAsync((InventoryProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.DeleteInventoryProductAsync(inventoryProductId, currentUserId));
            Assert.Equal($"InventoryProduct entry with ID {inventoryProductId} not found.", exception.Message);

            _mockInventoryProductRepository.Verify(repo => repo.Get(inventoryProductId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteInventoryProductAsync_ForbiddenAccess_ThrowsForbiddenException()
        {
            // Arrange
            var inventoryProductId = 1;
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = inventoryProductId, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse", IsDeleted = false };
            existingEntry.Inventory = inventory; // Set navigation property for access check

            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync(inventory); // To get inventory name for logging
            SetupManagerAccess(false, existingEntry.InventoryId, currentUserId, inventory); // User is not manager

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ForbiddenException>(() => _inventoryProductService.DeleteInventoryProductAsync(inventoryProductId, currentUserId));
            Assert.Equal($"User is not authorized to manage inventory '{inventory.Name}'.", exception.Message);

            _mockInventoryProductRepository.Verify(repo => repo.Get(inventoryProductId), Times.Once);
            _mockInventoryManagerRepository.Verify(repo => repo.IsUserManagerOfInventory(currentUserId, existingEntry.InventoryId), Times.Once);
            _mockInventoryProductRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"User {currentUserId} attempted forbidden operation on inventory '{inventory.Name}'.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once
            );
        }

        #endregion

        #region UpdateMinStockQuantityAsync Tests

        [Fact]
        public async Task UpdateMinStockQuantityAsync_ValidDto_UpdatesMinStockAndLogs()
        {
            // Arrange
            var dto = new UpdateInventoryProductMinStockDto { InventoryId = 1, ProductId = 1, NewMinStockQuantity = 10 };
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" };
            var product = new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 10, Inventory = inventory, Product = product };

            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(existingEntry));
            var oldValuesJson = JsonSerializer.Serialize(oldEntrySnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(updatedEntry, _jsonSerializerOptions);

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.Update(existingEntry.Id, It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.UpdateMinStockQuantityAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.MinStockQuantity);
            _mockInventoryProductRepository.Verify(repo => repo.Update(existingEntry.Id, It.Is<InventoryProduct>(ip => ip.MinStockQuantity == 10)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "MIN_STOCK_UPDATE" &&
                JsonSerializer.Serialize(log.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(log.NewValues, _jsonSerializerOptions) == newValuesJson &&
                log.Changes!.Contains("Updated MinStockQuantity from 5 to 10.")
            )), Times.Once);
            _mockClientProxy.Verify(x => x.SendCoreAsync("ReceiveLowStockNotification", It.IsAny<object[]>(), CancellationToken.None), Times.Never); // Not low stock
        }

        [Fact]
        public async Task UpdateMinStockQuantityAsync_ProductLowStockAfterUpdate_SendsNotification()
        {
            // Arrange
            var dto = new UpdateInventoryProductMinStockDto { InventoryId = 1, ProductId = 1, NewMinStockQuantity = 20 };
            var currentUserId = 101;
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Main Warehouse" };
            var product = new Product { ProductId = 1, ProductName = "Item A", SKU = "SKU001" };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 20, Inventory = inventory, Product = product }; // Now low stock

            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)existingEntry);
            _mockInventoryRepository.Setup(repo => repo.Get(existingEntry.InventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockProductRepository.Setup(repo => repo.Get(existingEntry.ProductId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.Update(existingEntry.Id, It.IsAny<InventoryProduct>())).ReturnsAsync((InventoryProduct?)updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());
            _mockClientProxy.Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), CancellationToken.None)).Returns(Task.CompletedTask);


            // Act
            var result = await _inventoryProductService.UpdateMinStockQuantityAsync(dto, currentUserId);

            // Assert
            Assert.NotNull(result);
            _mockClientProxy.Verify(x => x.SendCoreAsync(
                "ReceiveLowStockNotification",
                It.Is<object[]>(args =>
                    args.Length == 1 &&
                    (args[0] as LowStockNotificationDto)!.CurrentQuantity == 15 &&
                    (args[0] as LowStockNotificationDto)!.MinStockQuantity == 20
                ), CancellationToken.None), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(log =>
                log.ActionType == "LOW_STOCK_ALERT"
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateMinStockQuantityAsync_EntryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var dto = new UpdateInventoryProductMinStockDto { InventoryId = 1, ProductId = 1, NewMinStockQuantity = 10 };
            var currentUserId = 101;
            SetupManagerAccess(true, dto.InventoryId, currentUserId);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId)).ReturnsAsync((InventoryProduct?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.UpdateMinStockQuantityAsync(dto, currentUserId));
            Assert.Equal($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}. Cannot update min stock quantity.", exception.Message);
        }

        #endregion

        #region GetInventoryProductByIdAsync Tests

        [Fact]
        public async Task GetInventoryProductByIdAsync_Found_ReturnsDto()
        {
            // Arrange
            var id = 1;
            var entry = new InventoryProduct { Id = id, Inventory = new Inventory(), Product = new Product() };
            _mockInventoryProductRepository.Setup(repo => repo.Get(id)).ReturnsAsync((InventoryProduct?)entry);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task GetInventoryProductByIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var id = 99;
            _mockInventoryProductRepository.Setup(repo => repo.Get(id)).ReturnsAsync((InventoryProduct?)null);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByIdAsync(id);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetInventoryProductByInventoryAndProductIdAsync Tests

        [Fact]
        public async Task GetInventoryProductByInventoryAndProductIdAsync_Found_ReturnsDto()
        {
            // Arrange
            var inventoryId = 1;
            var productId = 1;
            var entry = new InventoryProduct { Id = 1, InventoryId = inventoryId, ProductId = productId, Inventory = new Inventory(), Product = new Product() };
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(inventoryId, productId)).ReturnsAsync((InventoryProduct?)entry);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByInventoryAndProductIdAsync(inventoryId, productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
            Assert.Equal(productId, result.ProductId);
        }

        [Fact]
        public async Task GetInventoryProductByInventoryAndProductIdAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var inventoryId = 1;
            var productId = 99;
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(inventoryId, productId)).ReturnsAsync((InventoryProduct?)null);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByInventoryAndProductIdAsync(inventoryId, productId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAllInventoryProductsAsync Tests

        [Fact]
        public async Task GetAllInventoryProductsAsync_ReturnsAll()
        {
            // Arrange
            var entries = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, Inventory = new Inventory(), Product = new Product() },
                new InventoryProduct { Id = 2, Inventory = new Inventory(), Product = new Product() }
            };
            _mockInventoryProductRepository.Setup(repo => repo.GetAll()).ReturnsAsync(entries);

            // Act
            var result = await _inventoryProductService.GetAllInventoryProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllInventoryProductsAsync_NoEntries_ReturnsEmptyList()
        {
            // Arrange
            _mockInventoryProductRepository.Setup(repo => repo.GetAll()).ReturnsAsync(new List<InventoryProduct>());

            // Act
            var result = await _inventoryProductService.GetAllInventoryProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetProductsInInventoryAsync Tests

        [Fact]
        public async Task GetProductsInInventoryAsync_InventoryFound_ReturnsProducts()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse A", IsDeleted = false };
            var productsInInventory = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = inventoryId, Product = new Product { ProductId = 1, ProductName = "Product 1", IsDeleted = false }, Inventory = inventory },
                new InventoryProduct { Id = 2, InventoryId = inventoryId, Product = new Product { ProductId = 2, ProductName = "Product 2", IsDeleted = false }, Inventory = inventory }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockInventoryProductRepository.Setup(repo => repo.GetProductsForInventory(inventoryId)).ReturnsAsync(productsInInventory);

            // Act
            var result = await _inventoryProductService.GetProductsInInventoryAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.ProductName == "Product 1");
        }

        [Fact]
        public async Task GetProductsInInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetProductsInInventoryAsync(inventoryId));
            Assert.Equal($"Inventory with ID {inventoryId} not found or is deleted.", exception.Message);
        }

        [Fact]
        public async Task GetProductsInInventoryAsync_InventoryDeleted_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 1;
            var deletedInventory = new Inventory { InventoryId = inventoryId, Name = "Deleted Warehouse", IsDeleted = true };
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)deletedInventory);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetProductsInInventoryAsync(inventoryId));
            Assert.Equal($"Inventory with ID {inventoryId} not found or is deleted.", exception.Message);
        }

        [Fact]
        public async Task GetProductsInInventoryAsync_IncludesOnlyActiveProducts()
        {
            // Arrange
            var inventoryId = 1;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse A", IsDeleted = false };
            var productsInInventory = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = inventoryId, Product = new Product { ProductId = 1, ProductName = "Active Product", IsDeleted = false }, Inventory = inventory },
                new InventoryProduct { Id = 2, InventoryId = inventoryId, Product = new Product { ProductId = 2, ProductName = "Deleted Product", IsDeleted = true }, Inventory = inventory }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockInventoryProductRepository.Setup(repo => repo.GetProductsForInventory(inventoryId)).ReturnsAsync(productsInInventory);

            // Act
            var result = await _inventoryProductService.GetProductsInInventoryAsync(inventoryId);

            // Assert
            Assert.Single(result); // Only active product should be returned
            Assert.Contains(result, p => p.ProductName == "Active Product");
            Assert.DoesNotContain(result, p => p.ProductName == "Deleted Product");
        }

        #endregion

        #region GetProductsInInventoryByCategoryAsync Tests

        [Fact]
        public async Task GetProductsInInventoryByCategoryAsync_ValidInputs_ReturnsProducts()
        {
            // Arrange
            var inventoryId = 1;
            var categoryId = 1;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse A", IsDeleted = false };
            var category = new Category { CategoryId = categoryId, CategoryName = "Electronics" };
            var productsInInventory = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = inventoryId, Product = new Product { ProductId = 1, ProductName = "Product 1", CategoryId = categoryId, Category = category, IsDeleted = false }, Inventory = inventory },
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockCategoryRepository.Setup(repo => repo.Get(categoryId)).ReturnsAsync((Category?)category);
            _mockInventoryProductRepository.Setup(repo => repo.GetProductsInInventoryByCategory(inventoryId, categoryId)).ReturnsAsync(productsInInventory);

            // Act
            var result = await _inventoryProductService.GetProductsInInventoryByCategoryAsync(inventoryId, categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, p => p.ProductName == "Product 1" && p.CategoryId == categoryId);
        }

        [Fact]
        public async Task GetProductsInInventoryByCategoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var categoryId = 1;
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetProductsInInventoryByCategoryAsync(inventoryId, categoryId));
            Assert.Equal($"Inventory with ID {inventoryId} not found or is deleted.", exception.Message);
        }

        [Fact]
        public async Task GetProductsInInventoryByCategoryAsync_CategoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 1;
            var categoryId = 99;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse A", IsDeleted = false };
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockCategoryRepository.Setup(repo => repo.Get(categoryId)).ReturnsAsync((Category?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetProductsInInventoryByCategoryAsync(inventoryId, categoryId));
            Assert.Equal($"Category with ID {categoryId} not found.", exception.Message);
        }

        #endregion

        #region GetInventoriesForProductAsync Tests

        [Fact]
        public async Task GetInventoriesForProductAsync_ProductFound_ReturnsInventories()
        {
            // Arrange
            var productId = 1;
            var product = new Product { ProductId = productId, ProductName = "Product A", IsDeleted = false };
            var inventoriesForProduct = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, ProductId = productId, Inventory = new Inventory { InventoryId = 1, Name = "Inv 1", IsDeleted = false }, Product = product },
                new InventoryProduct { Id = 2, ProductId = productId, Inventory = new Inventory { InventoryId = 2, Name = "Inv 2", IsDeleted = false }, Product = product }
            };

            _mockProductRepository.Setup(repo => repo.Get(productId)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.GetInventoriesForProduct(productId)).ReturnsAsync(inventoriesForProduct);

            // Act
            var result = await _inventoryProductService.GetInventoriesForProductAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, inv => inv.InventoryName == "Inv 1");
        }

        [Fact]
        public async Task GetInventoriesForProductAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var productId = 99;
            _mockProductRepository.Setup(repo => repo.Get(productId)).ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetInventoriesForProductAsync(productId));
            Assert.Equal($"Product with ID {productId} not found or is deleted.", exception.Message);
        }

        #endregion

        #region GetInventoriesForProductBySKUAsync Tests

        [Fact]
        public async Task GetInventoriesForProductBySKUAsync_ProductFound_ReturnsInventories()
        {
            // Arrange
            var sku = "SKU001";
            var productId = 1;
            var product = new Product { ProductId = productId, SKU = sku, ProductName = "Product A", IsDeleted = false };
            var inventoriesForProduct = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, ProductId = productId, Inventory = new Inventory { InventoryId = 1, Name = "Inv 1", IsDeleted = false }, Product = product },
            };

            _mockProductRepository.Setup(repo => repo.GetBySKU(sku)).ReturnsAsync((Product?)product);
            _mockInventoryProductRepository.Setup(repo => repo.GetInventoriesForProduct(productId)).ReturnsAsync(inventoriesForProduct);

            // Act
            var result = await _inventoryProductService.GetInventoriesForProductBySKUAsync(sku);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, inv => inv.InventoryName == "Inv 1");
        }

        [Fact]
        public async Task GetInventoriesForProductBySKUAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var sku = "NONEXISTENT";
            _mockProductRepository.Setup(repo => repo.GetBySKU(sku)).ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetInventoriesForProductBySKUAsync(sku));
            Assert.Equal($"Product with SKU '{sku}' not found or is deleted.", exception.Message);
        }

        #endregion

        #region GetLowStockProductsInInventoryAsync Tests

        [Fact]
        public async Task GetLowStockProductsInInventoryAsync_InventoryFound_ReturnsLowStockProducts()
        {
            // Arrange
            var inventoryId = 1;
            var threshold = 10;
            var inventory = new Inventory { InventoryId = inventoryId, Name = "Warehouse A", IsDeleted = false };
            var lowStockProducts = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = inventoryId, Quantity = 5, MinStockQuantity = 10, Product = new Product { ProductId = 1, ProductName = "Low Stock Item", IsDeleted = false, Category = new Category() }, Inventory = inventory }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)inventory);
            _mockInventoryProductRepository.Setup(repo => repo.GetLowStockProducts(inventoryId, threshold)).ReturnsAsync(lowStockProducts);

            // Act
            var result = await _inventoryProductService.GetLowStockProductsInInventoryAsync(inventoryId, threshold);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, p => p.ProductName == "Low Stock Item");
            Assert.Equal(5, result.First().QuantityInInventory);
        }

        [Fact]
        public async Task GetLowStockProductsInInventoryAsync_InventoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryId = 99;
            var threshold = 10;
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync((Inventory?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.GetLowStockProductsInInventoryAsync(inventoryId, threshold));
            Assert.Equal($"Inventory with ID {inventoryId} not found or is deleted.", exception.Message);
        }

        #endregion
    }
}