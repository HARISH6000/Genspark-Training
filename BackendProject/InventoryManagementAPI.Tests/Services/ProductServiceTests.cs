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
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IAuditLogService> _mockAuditLogService;
        private readonly ProductService _productService;

        // Define JsonSerializerOptions once to avoid CS0854 errors in expression trees
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockAuditLogService = new Mock<IAuditLogService>();

            _productService = new ProductService(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockAuditLogService.Object
            );
        }

        #region AddProductAsync Tests

        [Fact]
        public async Task AddProductAsync_NewProduct_ReturnsAddedProductDtoAndLogsAudit()
        {
            // Arrange
            var addProductDto = new AddProductDto { SKU = "SKU001", ProductName = "Laptop", UnitPrice = 1200.00m, CategoryId = 1 };
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var newProduct = new Product { ProductId = 1, SKU = "SKU001", ProductName = "Laptop", UnitPrice = 1200.00m, CategoryId = 1, Category = category, IsDeleted = false };
            var currentUserId = 1;

            _mockCategoryRepository.Setup(repo => repo.Get(addProductDto.CategoryId))
                                   .ReturnsAsync((Category?)category);
            _mockProductRepository.Setup(repo => repo.GetBySKU(addProductDto.SKU))
                                  .ReturnsAsync((Product?)null); // No existing product with this SKU
            _mockProductRepository.Setup(repo => repo.Add(It.IsAny<Product>()))
                                  .ReturnsAsync((Product?)newProduct); // Simulate successful addition
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto()); // Mock audit log

            // Act
            var result = await _productService.AddProductAsync(addProductDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(newProduct.ProductId, result.ProductId);
            Assert.Equal(newProduct.ProductName, result.ProductName);
            Assert.Equal(newProduct.SKU, result.SKU);

            _mockCategoryRepository.Verify(repo => repo.Get(addProductDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(addProductDto.SKU), Times.Once);
            _mockProductRepository.Verify(repo => repo.Add(It.Is<Product>(p => p.SKU == addProductDto.SKU && p.ProductName == addProductDto.ProductName)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Products" &&
                dto.RecordId == newProduct.ProductId.ToString() &&
                dto.ActionType == "INSERT" &&
                dto.OldValues == null &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == JsonSerializer.Serialize(newProduct, _jsonSerializerOptions)
            )), Times.Once);
        }

        [Fact]
        public async Task AddProductAsync_CategoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var addProductDto = new AddProductDto { SKU = "SKU001", ProductName = "Laptop", UnitPrice = 1200.00m, CategoryId = 99 }; // Non-existent category
            var currentUserId = 1;

            _mockCategoryRepository.Setup(repo => repo.Get(addProductDto.CategoryId))
                                   .ReturnsAsync((Category?)null); // Category not found

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _productService.AddProductAsync(addProductDto, currentUserId));
            Assert.Equal($"Category with ID {addProductDto.CategoryId} not found.", exception.Message);

            _mockCategoryRepository.Verify(repo => repo.Get(addProductDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(It.IsAny<string>()), Times.Never);
            _mockProductRepository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task AddProductAsync_ExistingSKU_ThrowsConflictException()
        {
            // Arrange
            var addProductDto = new AddProductDto { SKU = "SKU001", ProductName = "Laptop", UnitPrice = 1200.00m, CategoryId = 1 };
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var existingProduct = new Product { ProductId = 1, SKU = "SKU001", ProductName = "Existing Laptop", UnitPrice = 1000.00m, CategoryId = 1 };
            var currentUserId = 1;

            _mockCategoryRepository.Setup(repo => repo.Get(addProductDto.CategoryId))
                                   .ReturnsAsync((Category?)category);
            _mockProductRepository.Setup(repo => repo.GetBySKU(addProductDto.SKU))
                                  .ReturnsAsync(existingProduct); // Simulate existing product with same SKU

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _productService.AddProductAsync(addProductDto, currentUserId));
            Assert.Equal($"Product with SKU '{addProductDto.SKU}' already exists.", exception.Message);

            _mockCategoryRepository.Verify(repo => repo.Get(addProductDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(addProductDto.SKU), Times.Once);
            _mockProductRepository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Theory]
        [InlineData("UNIQUE constraint failed: Products.SKU")]
        [InlineData("duplicate key violates unique constraint \"IX_Products_SKU\"")]
        public async Task AddProductAsync_DbUpdateExceptionDueToUniqueSKU_ThrowsConflictException(string innerExceptionMessage)
        {
            // Arrange
            var addProductDto = new AddProductDto { SKU = "SKU001", ProductName = "Laptop", UnitPrice = 1200.00m, CategoryId = 1 };
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var currentUserId = 1;

            _mockCategoryRepository.Setup(repo => repo.Get(addProductDto.CategoryId))
                                   .ReturnsAsync((Category?)category);
            _mockProductRepository.Setup(repo => repo.GetBySKU(addProductDto.SKU))
                                  .ReturnsAsync((Product?)null);
            _mockProductRepository.Setup(repo => repo.Add(It.IsAny<Product>()))
                                  .ThrowsAsync(new DbUpdateException("Test DbUpdateException", new Exception(innerExceptionMessage)));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _productService.AddProductAsync(addProductDto, currentUserId));
            Assert.Contains($"Product with SKU '{addProductDto.SKU}' already exists. (Database constraint)", exception.Message);

            _mockCategoryRepository.Verify(repo => repo.Get(addProductDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(addProductDto.SKU), Times.Once);
            _mockProductRepository.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region GetProductByIdAsync Tests

        [Fact]
        public async Task GetProductByIdAsync_ProductExists_ReturnsProductDto()
        {
            // Arrange
            var productId = 1;
            var mockProduct = new Product { ProductId = productId, SKU = "SKU001", ProductName = "Test Product", CategoryId = 1, Category = new Category() };
            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)mockProduct);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal(mockProduct.ProductName, result.ProductName);
            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_ProductDoesNotExist_ReturnsNull()
        {
            // Arrange
            var productId = 99;
            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
        }

        #endregion

        #region GetAllProductsAsync Tests

        [Fact]
        public async Task GetAllProductsAsync_IncludeDeletedTrue_ReturnsAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Active Prod", IsDeleted = false, Category = new Category() },
                new Product { ProductId = 2, ProductName = "Deleted Prod", IsDeleted = true, Category = new Category() }
            };
            _mockProductRepository.Setup(repo => repo.GetAll())
                                  .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync(includeDeleted: true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, p => p.ProductName == "Active Prod");
            Assert.Contains(result, p => p.ProductName == "Deleted Prod");
            _mockProductRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_IncludeDeletedFalse_ReturnsOnlyActiveProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, ProductName = "Active Prod", IsDeleted = false, Category = new Category() },
                new Product { ProductId = 2, ProductName = "Deleted Prod", IsDeleted = true, Category = new Category() }
            };
            _mockProductRepository.Setup(repo => repo.GetAll())
                                  .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync(includeDeleted: false);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, p => p.ProductName == "Active Prod");
            Assert.DoesNotContain(result, p => p.ProductName == "Deleted Prod");
            _mockProductRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllProductsAsync_NoProducts_ReturnsEmptyList()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.GetAll())
                                  .ReturnsAsync(new List<Product>());

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            _mockProductRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        #endregion

        #region UpdateProductAsync Tests

        [Fact]
        public async Task UpdateProductAsync_ValidUpdate_ReturnsUpdatedProductDtoAndLogsAudit()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductId = 1, SKU = "UPD001", ProductName = "Updated Prod", UnitPrice = 1500m, CategoryId = 1 };
            var existingProduct = new Product { ProductId = 1, SKU = "OLD001", ProductName = "Old Prod", UnitPrice = 1000m, CategoryId = 1, Category = new Category() };
            var updatedProduct = new Product { ProductId = 1, SKU = "UPD001", ProductName = "Updated Prod", UnitPrice = 1500m, CategoryId = 1, Category = new Category(), IsDeleted = false };
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var currentUserId = 1;

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(existingProduct));
            var oldValuesJson = JsonSerializer.Serialize(oldProductSnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(updatedProduct, _jsonSerializerOptions);


            _mockProductRepository.Setup(repo => repo.Get(updateDto.ProductId))
                                  .ReturnsAsync((Product?)existingProduct);
            _mockCategoryRepository.Setup(repo => repo.Get(updateDto.CategoryId))
                                   .ReturnsAsync((Category?)category);
            _mockProductRepository.Setup(repo => repo.GetBySKU(updateDto.SKU))
                                  .ReturnsAsync((Product?)null); // No conflict with new SKU
            _mockProductRepository.Setup(repo => repo.Update(updateDto.ProductId, It.IsAny<Product>()))
                                  .ReturnsAsync((Product?)updatedProduct);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _productService.UpdateProductAsync(updateDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.ProductName, result.ProductName);
            Assert.Equal(updateDto.SKU, result.SKU);

            _mockProductRepository.Verify(repo => repo.Get(updateDto.ProductId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.Get(updateDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(updateDto.SKU), Times.Once); // Called because SKU changed
            _mockProductRepository.Verify(repo => repo.Update(updateDto.ProductId, It.Is<Product>(p => p.SKU == updateDto.SKU)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Products" &&
                dto.RecordId == updatedProduct.ProductId.ToString() &&
                dto.ActionType == "UPDATE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == newValuesJson
            )), Times.Once);
        }

        [Fact]
        public async Task UpdateProductAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductId = 99, SKU = "SKU999", ProductName = "NonExistent", CategoryId = 1 };
            var currentUserId = 1;

            _mockProductRepository.Setup(repo => repo.Get(updateDto.ProductId))
                                  .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _productService.UpdateProductAsync(updateDto, currentUserId));
            Assert.Equal($"Product with ID {updateDto.ProductId} not found.", exception.Message);

            _mockProductRepository.Verify(repo => repo.Get(updateDto.ProductId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.Get(It.IsAny<int>()), Times.Never);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_CategoryNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductId = 1, SKU = "UPD001", ProductName = "Updated Prod", UnitPrice = 1500m, CategoryId = 99 };
            var existingProduct = new Product { ProductId = 1, SKU = "OLD001", ProductName = "Old Prod", UnitPrice = 1000m, CategoryId = 1, Category = new Category() };
            var currentUserId = 1;

            _mockProductRepository.Setup(repo => repo.Get(updateDto.ProductId))
                                  .ReturnsAsync((Product?)existingProduct);
            _mockCategoryRepository.Setup(repo => repo.Get(updateDto.CategoryId))
                                   .ReturnsAsync((Category?)null); // Category not found for update

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _productService.UpdateProductAsync(updateDto, currentUserId));
            Assert.Equal($"Category with ID {updateDto.CategoryId} not found for update.", exception.Message);

            _mockProductRepository.Verify(repo => repo.Get(updateDto.ProductId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.Get(updateDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_SKUConflictsWithAnotherProduct_ThrowsConflictException()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductId = 1, SKU = "CONFLICT", ProductName = "Updated Prod", UnitPrice = 1500m, CategoryId = 1 };
            var existingProduct = new Product { ProductId = 1, SKU = "OLD_SKU", ProductName = "Old Prod", UnitPrice = 1000m, CategoryId = 1, Category = new Category() };
            var conflictingProduct = new Product { ProductId = 2, SKU = "CONFLICT", ProductName = "Another Prod", UnitPrice = 2000m, CategoryId = 1 };
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var currentUserId = 1;

            _mockProductRepository.Setup(repo => repo.Get(updateDto.ProductId))
                                  .ReturnsAsync((Product?)existingProduct);
            _mockCategoryRepository.Setup(repo => repo.Get(updateDto.CategoryId))
                                   .ReturnsAsync((Category?)category);
            _mockProductRepository.Setup(repo => repo.GetBySKU(updateDto.SKU))
                                  .ReturnsAsync(conflictingProduct); // SKU already exists for another product

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ConflictException>(() => _productService.UpdateProductAsync(updateDto, currentUserId));
            Assert.Equal($"Product with SKU '{updateDto.SKU}' already exists.", exception.Message);

            _mockProductRepository.Verify(repo => repo.Get(updateDto.ProductId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.Get(updateDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(updateDto.SKU), Times.Once);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProductAsync_NoSKUChange_PerformsUpdateWithoutSKUConflictCheck()
        {
            // Arrange
            var updateDto = new UpdateProductDto { ProductId = 1, SKU = "SAME_SKU", ProductName = "Updated Prod", UnitPrice = 1500m, CategoryId = 1 };
            var existingProduct = new Product { ProductId = 1, SKU = "SAME_SKU", ProductName = "Old Prod", UnitPrice = 1000m, CategoryId = 1, Category = new Category(), IsDeleted = false };
            var updatedProduct = new Product { ProductId = 1, SKU = "SAME_SKU", ProductName = "Updated Prod", UnitPrice = 1500m, CategoryId = 1, Category = new Category(), IsDeleted = false };
            var category = new Category { CategoryId = 1, CategoryName = "Electronics" };
            var currentUserId = 1;

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(existingProduct));
            var oldValuesJson = JsonSerializer.Serialize(oldProductSnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(updatedProduct, _jsonSerializerOptions);

            _mockProductRepository.Setup(repo => repo.Get(updateDto.ProductId))
                                  .ReturnsAsync((Product?)existingProduct);
            _mockCategoryRepository.Setup(repo => repo.Get(updateDto.CategoryId))
                                   .ReturnsAsync((Category?)category);
            // GetBySKU should NOT be called if SKU doesn't change
            _mockProductRepository.Setup(repo => repo.Update(updateDto.ProductId, It.IsAny<Product>()))
                                  .ReturnsAsync((Product?)updatedProduct);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _productService.UpdateProductAsync(updateDto, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updateDto.ProductName, result.ProductName);
            Assert.Equal(updateDto.SKU, result.SKU);

            _mockProductRepository.Verify(repo => repo.Get(updateDto.ProductId), Times.Once);
            _mockCategoryRepository.Verify(repo => repo.Get(updateDto.CategoryId), Times.Once);
            _mockProductRepository.Verify(repo => repo.GetBySKU(It.IsAny<string>()), Times.Never); // No SKU conflict check
            _mockProductRepository.Verify(repo => repo.Update(updateDto.ProductId, It.Is<Product>(p => p.SKU == updateDto.SKU)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Products" &&
                dto.RecordId == updatedProduct.ProductId.ToString() &&
                dto.ActionType == "UPDATE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == newValuesJson
            )), Times.Once);
        }

        #endregion

        #region SoftDeleteProductAsync Tests

        [Fact]
        public async Task SoftDeleteProductAsync_ProductExistsAndNotDeleted_SoftDeletesAndLogsAudit()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product { ProductId = productId, SKU = "PROD001", ProductName = "Test Soft Delete", IsDeleted = false, Category = new Category() };
            var softDeletedProduct = new Product { ProductId = productId, SKU = "PROD001", ProductName = "Test Soft Delete", IsDeleted = true, Category = new Category() };
            var currentUserId = 1;

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(existingProduct));
            var oldValuesJson = JsonSerializer.Serialize(oldProductSnapshot, _jsonSerializerOptions);
            var newValuesJson = JsonSerializer.Serialize(softDeletedProduct, _jsonSerializerOptions);

            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)existingProduct);
            _mockProductRepository.Setup(repo => repo.Update(productId, It.Is<Product>(p => p.IsDeleted == true)))
                                  .ReturnsAsync((Product?)softDeletedProduct);
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _productService.SoftDeleteProductAsync(productId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
            Assert.Equal(productId, result.ProductId);

            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Update(productId, It.Is<Product>(p => p.IsDeleted == true)), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Products" &&
                dto.RecordId == productId.ToString() &&
                dto.ActionType == "SOFT_DELETE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                JsonSerializer.Serialize(dto.NewValues, _jsonSerializerOptions) == newValuesJson
            )), Times.Once);
        }

        [Fact]
        public async Task SoftDeleteProductAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var productId = 99;
            var currentUserId = 1;

            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _productService.SoftDeleteProductAsync(productId, currentUserId));
            Assert.Equal($"Product with ID {productId} not found.", exception.Message);

            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        [Fact]
        public async Task SoftDeleteProductAsync_ProductAlreadyDeleted_ReturnsExistingDeletedProductDtoWithoutReDeleting()
        {
            // Arrange
            var productId = 1;
            var alreadyDeletedProduct = new Product { ProductId = productId, SKU = "PROD001", ProductName = "Already Deleted", IsDeleted = true, Category = new Category() };
            var currentUserId = 1;

            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)alreadyDeletedProduct);

            // Act
            var result = await _productService.SoftDeleteProductAsync(productId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsDeleted);
            Assert.Equal(productId, result.ProductId);

            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Update(It.IsAny<int>(), It.IsAny<Product>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion

        #region HardDeleteProductAsync Tests

        [Fact]
        public async Task HardDeleteProductAsync_ProductExists_DeletesProductAndLogsAudit()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product { ProductId = productId, SKU = "PROD001", ProductName = "Test Hard Delete", IsDeleted = false, Category = new Category() };
            var currentUserId = 1;

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(existingProduct));
            var oldValuesJson = JsonSerializer.Serialize(oldProductSnapshot, _jsonSerializerOptions);

            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)existingProduct);
            _mockProductRepository.Setup(repo => repo.Delete(productId))
                                  .ReturnsAsync((Product?)existingProduct); // Simulate successful hard delete
            _mockAuditLogService.Setup(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()))
                                .ReturnsAsync(new AuditLogResponseDto());

            // Act
            var result = await _productService.HardDeleteProductAsync(productId, currentUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal(existingProduct.ProductName, result.ProductName);

            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Delete(productId), Times.Once);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.Is<AuditLogEntryDto>(dto =>
                dto.UserId == currentUserId &&
                dto.TableName == "Products" &&
                dto.RecordId == productId.ToString() &&
                dto.ActionType == "HARD_DELETE" &&
                JsonSerializer.Serialize(dto.OldValues, _jsonSerializerOptions) == oldValuesJson &&
                dto.NewValues == null
            )), Times.Once);
        }

        [Fact]
        public async Task HardDeleteProductAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var productId = 99;
            var currentUserId = 1;

            _mockProductRepository.Setup(repo => repo.Get(productId))
                                  .ReturnsAsync((Product?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<NotFoundException>(() => _productService.HardDeleteProductAsync(productId, currentUserId));
            Assert.Equal($"Product with ID {productId} not found.", exception.Message);

            _mockProductRepository.Verify(repo => repo.Get(productId), Times.Once);
            _mockProductRepository.Verify(repo => repo.Delete(It.IsAny<int>()), Times.Never);
            _mockAuditLogService.Verify(service => service.LogActionAsync(It.IsAny<AuditLogEntryDto>()), Times.Never);
        }

        #endregion
    }
}