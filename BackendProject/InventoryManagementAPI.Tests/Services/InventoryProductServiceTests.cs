using Xunit;
using Moq;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Services;
using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace InventoryManagementAPI.Tests.Services
{
    public class InventoryProductServiceTests
    {
        private readonly Mock<IInventoryProductRepository> _mockInventoryProductRepository;
        private readonly Mock<IInventoryRepository> _mockInventoryRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly Mock<IHubContext<InventoryManagementAPI.Hubs.LowStockHub>> _mockHubContext;
        private readonly Mock<IInventoryManagerRepository> _mockInventoryManagerRepository;
        private readonly Mock<ILogger<InventoryProductService>> _mockLogger;

        private readonly InventoryProductService _inventoryProductService;

        public InventoryProductServiceTests()
        {
            _mockInventoryProductRepository = new Mock<IInventoryProductRepository>();
            _mockInventoryRepository = new Mock<IInventoryRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockAuditLogService = new Mock<IAuditLogService>();
            _mockHubContext = new Mock<IHubContext<InventoryManagementAPI.Hubs.LowStockHub>>();
            _mockInventoryManagerRepository = new Mock<IInventoryManagerRepository>();
            _mockLogger = new Mock<ILogger<InventoryProductService>>();

            // Mock the Clients.All property for SignalR hub
            _mockHubContext.Setup(x => x.Clients.All)
                           .Returns(Mock.Of<IClientProxy>());

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

            // Setup common mock behaviors for authorization
            _mockInventoryManagerRepository.Setup(repo => repo.IsUserManagerOfInventory(It.IsAny<int>(), It.IsAny<int>()))
                                           .ReturnsAsync(true);
            _mockInventoryRepository.Setup(repo => repo.Get(It.IsAny<int>()))
                                    .ReturnsAsync(new Inventory { InventoryId = 1, Name = "Test Inventory", IsDeleted = false });
            _mockProductRepository.Setup(repo => repo.Get(It.IsAny<int>()))
                                  .ReturnsAsync(new Product { ProductId = 1, ProductName = "Test Product", SKU = "SKU001", CategoryId = 1, IsDeleted = false });
            _mockCategoryRepository.Setup(repo => repo.Get(It.IsAny<int>()))
                                   .ReturnsAsync(new Category { CategoryId = 1, CategoryName = "Test Category" });
        }

        #region AddInventoryProductAsync Tests

        [Fact]
        public async Task AddInventoryProductAsync_ValidData_ReturnsResponseDto()
        {
            // Arrange
            var addDto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var addedIp = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Test Inventory", IsDeleted = false };
            var product = new Product { ProductId = 1, ProductName = "Test Product", SKU = "SKU001", CategoryId = 1, IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.Get(addDto.InventoryId)).ReturnsAsync(inventory);
            _mockProductRepository.Setup(repo => repo.Get(addDto.ProductId)).ReturnsAsync(product);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(addDto.InventoryId, addDto.ProductId))
                                           .ReturnsAsync((InventoryProduct)null);
            _mockInventoryProductRepository.Setup(repo => repo.Add(It.IsAny<InventoryProduct>())).ReturnsAsync(addedIp);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());


            // Act
            var result = await _inventoryProductService.AddInventoryProductAsync(addDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(addedIp.Id, result.Id);
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        [Fact]
        public async Task AddInventoryProductAsync_ExistingEntry_ThrowsConflictException()
        {
            // Arrange
            var addDto = new AddInventoryProductDto { InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var existingEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 5 };
            var inventory = new Inventory { InventoryId = 1, Name = "Test Inventory", IsDeleted = false };
            var product = new Product { ProductId = 1, ProductName = "Test Product", SKU = "SKU001", CategoryId = 1, IsDeleted = false };

            _mockInventoryRepository.Setup(repo => repo.Get(addDto.InventoryId)).ReturnsAsync(inventory);
            _mockProductRepository.Setup(repo => repo.Get(addDto.ProductId)).ReturnsAsync(product);
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(addDto.InventoryId, addDto.ProductId))
                                           .ReturnsAsync(existingEntry);

            // Act & Assert
            await Assert.ThrowsAsync<ConflictException>(() => _inventoryProductService.AddInventoryProductAsync(addDto, 1));
            _mockInventoryProductRepository.Verify(repo => repo.Add(It.IsAny<InventoryProduct>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region IncreaseProductQuantityAsync Tests

        [Fact]
        public async Task IncreaseProductQuantityAsync_ValidData_IncreasesQuantity()
        {
            // Arrange
            var adjustDto = new AdjustProductQuantityDto { InventoryId = 1, ProductId = 1, QuantityChange = 5 };
            var initialEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 5 };

            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(adjustDto.InventoryId, adjustDto.ProductId))
                                           .ReturnsAsync(initialEntry);
            _mockInventoryProductRepository.Setup(repo => repo.Update(initialEntry.Id, It.IsAny<InventoryProduct>()))
                                           .ReturnsAsync(updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryProductService.IncreaseProductQuantityAsync(adjustDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedEntry.Quantity, result.Quantity);
            _mockInventoryProductRepository.Verify(repo => repo.Update(initialEntry.Id, It.Is<InventoryProduct>(ip => ip.Quantity == 15)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        #endregion

        #region SetProductQuantityAsync Tests

        [Fact]
        public async Task SetProductQuantityAsync_ValidData_SetsQuantity()
        {
            // Arrange
            var setDto = new SetProductQuantityDto { InventoryId = 1, ProductId = 1, NewQuantity = 20 };
            var initialEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 20, MinStockQuantity = 5 };

            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(setDto.InventoryId, setDto.ProductId))
                                           .ReturnsAsync(initialEntry);
            _mockInventoryProductRepository.Setup(repo => repo.Update(initialEntry.Id, It.IsAny<InventoryProduct>()))
                                           .ReturnsAsync(updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryProductService.SetProductQuantityAsync(setDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedEntry.Quantity, result.Quantity);
            _mockInventoryProductRepository.Verify(repo => repo.Update(initialEntry.Id, It.Is<InventoryProduct>(ip => ip.Quantity == 20)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        #endregion

        #region DeleteInventoryProductAsync Tests

        [Fact]
        public async Task DeleteInventoryProductAsync_ExistingEntry_DeletesEntry()
        {
            // Arrange
            var inventoryProductId = 1;
            var entryToDelete = new InventoryProduct { Id = inventoryProductId, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5,
                Inventory = new Inventory { InventoryId = 1, Name = "Test Inventory" }};

            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId))
                                           .ReturnsAsync(entryToDelete);
            _mockInventoryProductRepository.Setup(repo => repo.Delete(inventoryProductId))
                                           .ReturnsAsync(entryToDelete);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _inventoryProductService.DeleteInventoryProductAsync(inventoryProductId, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(entryToDelete.Id, result.Id);
            _mockInventoryProductRepository.Verify(repo => repo.Delete(inventoryProductId), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        [Fact]
        public async Task DeleteInventoryProductAsync_NonExistingEntry_ThrowsNotFoundException()
        {
            // Arrange
            var inventoryProductId = 99;
            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId))
                                           .ReturnsAsync((InventoryProduct)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _inventoryProductService.DeleteInventoryProductAsync(inventoryProductId, 1));
            _mockInventoryProductRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region UpdateMinStockQuantityAsync Tests

        [Fact]
        public async Task UpdateMinStockQuantityAsync_ValidData_UpdatesMinStockQuantity()
        {
            // Arrange
            var updateDto = new UpdateInventoryProductMinStockDto { InventoryId = 1, ProductId = 1, NewMinStockQuantity = 10 };
            var initialEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 5 };
            var updatedEntry = new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 15, MinStockQuantity = 10 };

            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(updateDto.InventoryId, updateDto.ProductId))
                                           .ReturnsAsync(initialEntry);
            _mockInventoryProductRepository.Setup(repo => repo.Update(initialEntry.Id, It.IsAny<InventoryProduct>()))
                                           .ReturnsAsync(updatedEntry);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>())).ReturnsAsync(new AuditLogResponseDto());


            // Act
            var result = await _inventoryProductService.UpdateMinStockQuantityAsync(updateDto, 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedEntry.MinStockQuantity, result.MinStockQuantity);
            _mockInventoryProductRepository.Verify(repo => repo.Update(initialEntry.Id, It.Is<InventoryProduct>(ip => ip.MinStockQuantity == 10)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Once);
        }

        #endregion

        #region GetInventoryProductByIdAsync Tests

        [Fact]
        public async Task GetInventoryProductByIdAsync_ExistingId_ReturnsResponseDto()
        {
            // Arrange
            var inventoryProductId = 1;
            var inventoryProduct = new InventoryProduct { Id = inventoryProductId, InventoryId = 1, ProductId = 1, Quantity = 10, MinStockQuantity = 5 };
            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId)).ReturnsAsync(inventoryProduct);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByIdAsync(inventoryProductId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryProductId, result.Id);
        }

        [Fact]
        public async Task GetInventoryProductByIdAsync_NonExistingId_ReturnsNull()
        {
            // Arrange
            var inventoryProductId = 99;
            _mockInventoryProductRepository.Setup(repo => repo.Get(inventoryProductId)).ReturnsAsync((InventoryProduct)null);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByIdAsync(inventoryProductId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetInventoryProductByInventoryAndProductIdAsync Tests

        [Fact]
        public async Task GetInventoryProductByInventoryAndProductIdAsync_ExistingEntry_ReturnsResponseDto()
        {
            // Arrange
            var inventoryId = 1;
            var productId = 1;
            var inventoryProduct = new InventoryProduct { Id = 1, InventoryId = inventoryId, ProductId = productId, Quantity = 10, MinStockQuantity = 5 };
            _mockInventoryProductRepository.Setup(repo => repo.GetByInventoryAndProductId(inventoryId, productId)).ReturnsAsync(inventoryProduct);

            // Act
            var result = await _inventoryProductService.GetInventoryProductByInventoryAndProductIdAsync(inventoryId, productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(inventoryId, result.InventoryId);
            Assert.Equal(productId, result.ProductId);
        }

        #endregion

        #region GetAllInventoryProductsAsync Tests

        [Fact]
        public async Task GetAllInventoryProductsAsync_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10 },
                new InventoryProduct { Id = 2, InventoryId = 1, ProductId = 2, Quantity = 5 }
            };
            _mockInventoryProductRepository.Setup(repo => repo.GetAll()).ReturnsAsync(products);

            // Act
            var result = await _inventoryProductService.GetAllInventoryProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetProductsInInventoryAsync Tests

        [Fact]
        public async Task GetProductsInInventoryAsync_ValidInventory_ReturnsProducts()
        {
            // Arrange
            var inventoryId = 1;
            var productsInInventory = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, Product = new Product { ProductId = 1, ProductName = "Product A", IsDeleted = false } },
                new InventoryProduct { Id = 2, InventoryId = 1, ProductId = 2, Quantity = 5, Product = new Product { ProductId = 2, ProductName = "Product B", IsDeleted = false } }
            };
            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(new Inventory { InventoryId = inventoryId, IsDeleted = false });
            _mockInventoryProductRepository.Setup(repo => repo.GetProductsForInventory(inventoryId)).ReturnsAsync(productsInInventory);

            // Act
            var result = await _inventoryProductService.GetProductsInInventoryAsync(inventoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetProductsInInventoryByCategoryAsync Tests

        [Fact]
        public async Task GetProductsInInventoryByCategoryAsync_ValidData_ReturnsFilteredProducts()
        {
            // Arrange
            var inventoryId = 1;
            var categoryId = 1;
            var products = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, Product = new Product { ProductId = 1, CategoryId = 1, IsDeleted = false } },
                new InventoryProduct { Id = 2, InventoryId = 1, ProductId = 2, Quantity = 5, Product = new Product { ProductId = 2, CategoryId = 1, IsDeleted = false } }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(new Inventory { InventoryId = inventoryId, IsDeleted = false });
            _mockCategoryRepository.Setup(repo => repo.Get(categoryId)).ReturnsAsync(new Category { CategoryId = categoryId });
            _mockInventoryProductRepository.Setup(repo => repo.GetProductsInInventoryByCategory(inventoryId, categoryId)).ReturnsAsync(products);

            // Act
            var result = await _inventoryProductService.GetProductsInInventoryByCategoryAsync(inventoryId, categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetInventoriesForProductAsync Tests

        [Fact]
        public async Task GetInventoriesForProductAsync_ValidProduct_ReturnsInventories()
        {
            // Arrange
            var productId = 1;
            var inventories = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, Inventory = new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false } },
                new InventoryProduct { Id = 2, InventoryId = 2, ProductId = 1, Quantity = 5, Inventory = new Inventory { InventoryId = 2, Name = "Warehouse B", IsDeleted = false } }
            };
            _mockProductRepository.Setup(repo => repo.Get(productId)).ReturnsAsync(new Product { ProductId = productId, IsDeleted = false });
            _mockInventoryProductRepository.Setup(repo => repo.GetInventoriesForProduct(productId)).ReturnsAsync(inventories);

            // Act
            var result = await _inventoryProductService.GetInventoriesForProductAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion

        #region GetInventoriesForProductBySKUAsync Tests

        [Fact]
        public async Task GetInventoriesForProductBySKUAsync_ValidSKU_ReturnsInventories()
        {
            // Arrange
            var sku = "SKU001";
            var product = new Product { ProductId = 1, SKU = sku, IsDeleted = false };
            var inventories = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 10, Inventory = new Inventory { InventoryId = 1, Name = "Warehouse A", IsDeleted = false } },
            };

            _mockProductRepository.Setup(repo => repo.GetBySKU(sku)).ReturnsAsync(product);
            _mockInventoryProductRepository.Setup(repo => repo.GetInventoriesForProduct(product.ProductId)).ReturnsAsync(inventories);

            // Act
            var result = await _inventoryProductService.GetInventoriesForProductBySKUAsync(sku);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        #endregion

        #region GetLowStockProductsInInventoryAsync Tests

        [Fact]
        public async Task GetLowStockProductsInInventoryAsync_ValidData_ReturnsLowStockProducts()
        {
            // Arrange
            var inventoryId = 1;
            var threshold = 5;
            var lowStockEntries = new List<InventoryProduct>
            {
                new InventoryProduct { Id = 1, InventoryId = 1, ProductId = 1, Quantity = 3, MinStockQuantity = 5, Product = new Product { ProductId = 1, ProductName = "Low Stock A", IsDeleted = false } },
                new InventoryProduct { Id = 2, InventoryId = 1, ProductId = 2, Quantity = 5, MinStockQuantity = 5, Product = new Product { ProductId = 2, ProductName = "Low Stock B", IsDeleted = false } }
            };

            _mockInventoryRepository.Setup(repo => repo.Get(inventoryId)).ReturnsAsync(new Inventory { InventoryId = inventoryId, IsDeleted = false });
            _mockInventoryProductRepository.Setup(repo => repo.GetLowStockProducts(inventoryId, threshold)).ReturnsAsync(lowStockEntries);

            // Act
            var result = await _inventoryProductService.GetLowStockProductsInInventoryAsync(inventoryId, threshold);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.True(result.All(p => p.QuantityInInventory <= threshold));
        }

        #endregion
    }
}
