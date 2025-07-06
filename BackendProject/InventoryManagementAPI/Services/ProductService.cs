using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using InventoryManagementAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;

namespace InventoryManagementAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditLogService _auditLogService;

        public ProductService(IProductRepository productRepository,
                              ICategoryRepository categoryRepository,
                              IAuditLogService auditLogService)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _auditLogService = auditLogService;
        }

        public async Task<ProductResponseDto> AddProductAsync(AddProductDto productDto, int? currentUserId)
        {
            var category = await _categoryRepository.Get(productDto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {productDto.CategoryId} not found.");
            }

            var existingProduct = await _productRepository.GetBySKU(productDto.SKU);
            if (existingProduct != null)
            {
                throw new ConflictException($"Product with SKU '{productDto.SKU}' already exists.");
            }

            var newProduct = ProductMapper.ToProduct(productDto);
            newProduct.Category = category;

            try
            {
                var addedProduct = await _productRepository.Add(newProduct);

                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Products",
                    RecordId = addedProduct.ProductId.ToString(),
                    ActionType = "INSERT",
                    NewValues = addedProduct
                });

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
            return ProductMapper.ToProductResponseDto(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(bool includeDeleted = false)
        {
            var products = await _productRepository.GetAll();

            if (!includeDeleted)
            {
                products = products.Where(p => !p.IsDeleted);
            }
            return products.Select(p => ProductMapper.ToProductResponseDto(p));
        }

        public async Task<PaginationResponse<ProductResponseDto>> GetAllProductsAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null, bool includeDeleted = false)
        {

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);


            IQueryable<Product> query = _productRepository.GetAllAsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(p => !p.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(searchTerm) ||
                    p.SKU.ToLower().Contains(searchTerm)||p.Category.CategoryName.ToLower().Contains(searchTerm));
            }

            int totalRecords = await query.CountAsync();


            query = query.ApplyDatabaseSorting(orderBy, "ProductId");

            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var productResponseDtos = products.Select(p => ProductMapper.ToProductResponseDto(p));

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var paginationMetadata = new PaginationMetadata
            {
                TotalRecords = totalRecords,
                Page = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new PaginationResponse<ProductResponseDto>
            {
                Data = productResponseDtos,
                Pagination = paginationMetadata
            };
        }

        public async Task<ProductResponseDto> UpdateProductAsync(UpdateProductDto productDto, int? currentUserId)
        {
            var existingProduct = await _productRepository.Get(productDto.ProductId);
            if (existingProduct == null)
            {
                throw new NotFoundException($"Product with ID {productDto.ProductId} not found.");
            }

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(existingProduct));

            var category = await _categoryRepository.Get(productDto.CategoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {productDto.CategoryId} not found for update.");
            }

            if (existingProduct.SKU != productDto.SKU)
            {
                var productWithSameSku = await _productRepository.GetBySKU(productDto.SKU);
                if (productWithSameSku != null && productWithSameSku.ProductId != existingProduct.ProductId)
                {
                    throw new ConflictException($"Product with SKU '{productDto.SKU}' already exists.");
                }
            }

            ProductMapper.ToProduct(productDto, existingProduct);
            existingProduct.Category = category;

            try
            {
                var updatedProduct = await _productRepository.Update(productDto.ProductId, existingProduct);

                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "Products",
                    RecordId = updatedProduct.ProductId.ToString(),
                    ActionType = "UPDATE",
                    OldValues = oldProductSnapshot,
                    NewValues = updatedProduct
                });

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

        public async Task<ProductResponseDto> SoftDeleteProductAsync(int productId, int? currentUserId)
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

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(productToDelete));

            productToDelete.IsDeleted = true;
            var deletedProduct = await _productRepository.Update(productId, productToDelete);

            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Products",
                RecordId = productId.ToString(),
                ActionType = "SOFT_DELETE",
                OldValues = oldProductSnapshot,
                NewValues = deletedProduct
            });

            return ProductMapper.ToProductResponseDto(deletedProduct);
        }

        public async Task<ProductResponseDto> HardDeleteProductAsync(int productId, int? currentUserId)
        {
            var productToDelete = await _productRepository.Get(productId);
            if (productToDelete == null)
            {
                throw new NotFoundException($"Product with ID {productId} not found.");
            }

            var oldProductSnapshot = JsonSerializer.Deserialize<Product>(JsonSerializer.Serialize(productToDelete));

            var deletedProduct = await _productRepository.Delete(productId);

            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "Products",
                RecordId = productId.ToString(),
                ActionType = "HARD_DELETE",
                OldValues = oldProductSnapshot,
                NewValues = null
            });

            return ProductMapper.ToProductResponseDto(deletedProduct);
        }
    }
}
