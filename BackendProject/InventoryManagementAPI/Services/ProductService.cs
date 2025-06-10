using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagementAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IRepository<int, Category> _categoryRepository;
        // private readonly IAuditLogService _auditLogService;

        public ProductService(IProductRepository productRepository, IRepository<int, Category> categoryRepository) 
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            // _auditLogService = auditLogService;
        }

        public async Task<ProductResponseDto> AddProductAsync(AddProductDto productDto)
        {
            // 1. Validate CategoryId
            var category = await _categoryRepository.Get(productDto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {productDto.CategoryId} not found.");
            }

            // 2. Check for unique SKU
            var existingProduct = await _productRepository.GetBySKU(productDto.SKU);
            if (existingProduct != null)
            {
                throw new ConflictException($"Product with SKU '{productDto.SKU}' already exists.");
            }

            // 3. Map DTO to Product model and attach category
            var newProduct = ProductMapper.ToProduct(productDto);
            newProduct.Category = category; // Attach category object for EF

            // 4. Add product to database
            try
            {
                var addedProduct = await _productRepository.Add(newProduct);
                // _auditLogService.LogActionAsync(currentUserId, "Product", addedProduct.ProductId, "INSERT", null, addedProduct);
                return ProductMapper.ToProductResponseDto(addedProduct);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("SKU") || ex.InnerException.Message.Contains("sku"))
                        throw new ConflictException($"Product with SKU '{productDto.SKU}' already exists. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.Get(productId);
            if (product == null)
            {
                return null;
            }
            // Category should be eager-loaded by the repository Get method, so it's available for mapping
            return ProductMapper.ToProductResponseDto(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool includeDeleted = false)
        {
            var products = await _productRepository.GetAll();

            if (!includeDeleted)
            {
                products = products.Where(p => !p.IsDeleted);
            }
            // Categories should be eager-loaded by the repository GetAll method
            return products.Select(p => ProductMapper.ToProductResponseDto(p));
        }

        public async Task<ProductResponseDto> UpdateProductAsync(UpdateProductDto productDto)
        {
            var existingProduct = await _productRepository.Get(productDto.ProductId);
            if (existingProduct == null)
            {
                throw new NotFoundException($"Product with ID {productDto.ProductId} not found.");
            }

            // Validate CategoryId for update
            var category = await _categoryRepository.Get(productDto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {productDto.CategoryId} not found for update.");
            }

            // Check for SKU uniqueness if SKU is being changed
            if (existingProduct.SKU != productDto.SKU)
            {
                var productWithSameSku = await _productRepository.GetBySKU(productDto.SKU);
                if (productWithSameSku != null && productWithSameSku.ProductId != existingProduct.ProductId)
                {
                    throw new ConflictException($"Product with SKU '{productDto.SKU}' already exists.");
                }
            }

            // Map DTO properties to the existing product entity and update category
            ProductMapper.ToProduct(productDto, existingProduct);
            existingProduct.Category = category; // Update navigation property

            try
            {
                var updatedProduct = await _productRepository.Update(productDto.ProductId, existingProduct);
                // _auditLogService.LogActionAsync(currentUserId, "Product", updatedProduct.ProductId, "UPDATE", oldValues, updatedProduct);
                return ProductMapper.ToProductResponseDto(updatedProduct);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    if (ex.InnerException.Message.Contains("SKU") || ex.InnerException.Message.Contains("sku"))
                        throw new ConflictException($"Product with SKU '{productDto.SKU}' already exists. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<ProductResponseDto> SoftDeleteProductAsync(int productId)
        {
            var productToDelete = await _productRepository.Get(productId);
            if (productToDelete == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            if (productToDelete.IsDeleted)
            {
                return ProductMapper.ToProductResponseDto(productToDelete);
            }

            productToDelete.IsDeleted = true;
            var deletedProduct = await _productRepository.Update(productId, productToDelete);
            // _auditLogService.LogActionAsync(currentUserId, "Product", productId, "SOFT_DELETE", productToDelete, deletedProduct);
            return ProductMapper.ToProductResponseDto(deletedProduct);
        }

        public async Task<ProductResponseDto> HardDeleteProductAsync(int productId)
        {
            var productToDelete = await _productRepository.Get(productId);
            if (productToDelete == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            var deletedProduct = await _productRepository.Delete(productId);
            // _auditLogService.LogActionAsync(currentUserId, "Product", productId, "HARD_DELETE", productToDelete, null);
            return ProductMapper.ToProductResponseDto(deletedProduct);
        }
    }
}
