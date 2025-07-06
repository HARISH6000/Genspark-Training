using InventoryManagementAPI.DTOs;
using InventoryManagementAPI.Interfaces;
using InventoryManagementAPI.Models;
using InventoryManagementAPI.Exceptions;
using InventoryManagementAPI.Mappers;
using InventoryManagementAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using InventoryManagementAPI.Hubs;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Services
{
    public class InventoryProductService : IInventoryProductService
    {
        private readonly IInventoryProductRepository _inventoryProductRepository;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly IHubContext<LowStockHub> _hubContext;
        private readonly IInventoryManagerRepository _inventoryManagerRepository;
        private readonly ILogger<InventoryProductService> _logger;

        public InventoryProductService(
            IInventoryProductRepository inventoryProductRepository,
            IInventoryRepository inventoryRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IAuditLogService auditLogService,
            IHubContext<LowStockHub> hubContext,
            IInventoryManagerRepository inventoryManagerRepository,
            ILogger<InventoryProductService> logger)
        {
            _inventoryProductRepository = inventoryProductRepository;
            _inventoryRepository = inventoryRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _auditLogService = auditLogService;
            _hubContext = hubContext;
            _inventoryManagerRepository = inventoryManagerRepository;
            _logger = logger;
        }

        private async Task CheckInventoryManagerAccess(int inventoryId, int? userId)
        {
            if (!userId.HasValue)
            {
                throw new ForbiddenException("User ID is missing. Unable to verify inventory management permissions.");
            }

            var isManager = await _inventoryManagerRepository.IsUserManagerOfInventory(userId.Value, inventoryId);
            if (!isManager)
            {
                var inventory = await _inventoryRepository.Get(inventoryId);
                var inventoryName = inventory?.Name ?? $"Inventory ID {inventoryId}";
                _logger.LogWarning("User {UserId} attempted forbidden operation on inventory '{InventoryName}'.", userId.Value, inventoryName);
                throw new ForbiddenException($"User is not authorized to manage inventory '{inventoryName}'.");
            }
            _logger.LogInformation("User {UserId} authorized for operation on inventory ID {InventoryId}.", userId.Value, inventoryId);
        }
        
        private async Task LoadNavigationProperties(InventoryProduct entry)
        {
            // Ensure Inventory and Product navigation properties are loaded
            if (entry.Inventory == null)
            {
                entry.Inventory = await _inventoryRepository.Get(entry.InventoryId);
            }
            if (entry.Product == null)
            {
                entry.Product = await _productRepository.Get(entry.ProductId);
            }
        }
        
        private async Task CheckAndNotifyLowStock(InventoryProduct inventoryProduct, int? currentUserId)
        {
            await LoadNavigationProperties(inventoryProduct);

            if (inventoryProduct.Product != null && inventoryProduct.Inventory != null)
            {
                if (inventoryProduct.Quantity <= inventoryProduct.MinStockQuantity)
                {
                    var notification = new LowStockNotificationDto
                    {
                        ProductId = inventoryProduct.ProductId,
                        ProductName = inventoryProduct.Product.ProductName,
                        SKU = inventoryProduct.Product.SKU,
                        CurrentQuantity = inventoryProduct.Quantity,
                        MinStockQuantity = inventoryProduct.MinStockQuantity,
                        InventoryId = inventoryProduct.InventoryId,
                        InventoryName = inventoryProduct.Inventory.Name,
                        Message = $"Low stock alert! {inventoryProduct.Product.ProductName} (SKU: {inventoryProduct.Product.SKU}) in {inventoryProduct.Inventory.Name} is at {inventoryProduct.Quantity} units, which is at or below the minimum of {inventoryProduct.MinStockQuantity}.",
                        Timestamp = DateTime.UtcNow
                    };
                    await _hubContext.Clients.All.SendAsync("ReceiveLowStockNotification", notification);


                    await _auditLogService.LogActionAsync(new AuditLogEntryDto
                    {
                        UserId = currentUserId,
                        TableName = "InventoryProducts",
                        RecordId = inventoryProduct.Id.ToString(),
                        ActionType = "LOW_STOCK_ALERT",
                        NewValues = notification
                    });
                }
            }
        }

        public async Task<InventoryProductResponseDto> AddInventoryProductAsync(AddInventoryProductDto dto, int? currentUserId)
        {
            await CheckInventoryManagerAccess(dto.InventoryId, currentUserId);
            if (dto.Quantity < 0)
            {
                throw new ArgumentException("Quantity cannot be negative.");
            }
            var inventory = await _inventoryRepository.Get(dto.InventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {dto.InventoryId} not found or is deleted.");
            }
            var product = await _productRepository.Get(dto.ProductId);
            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with ID {dto.ProductId} not found or is deleted.");
            }

            var existingEntry = await _inventoryProductRepository.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId);
            if (existingEntry != null)
            {
                throw new ConflictException($"Product '{product.ProductName}' (SKU: {product.SKU}) is already associated with inventory '{inventory.Name}'. Consider updating its quantity instead.");
            }

            var newEntry = InventoryProductMapper.ToInventoryProduct(dto);
            newEntry.Inventory = inventory;
            newEntry.Product = product;

            try
            {
                var addedEntry = await _inventoryProductRepository.Add(newEntry);

                // AUDIT LOGGING: INSERT
                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "InventoryProducts",
                    RecordId = addedEntry.Id.ToString(),
                    ActionType = "INSERT",
                    NewValues = addedEntry
                });
                
                await CheckAndNotifyLowStock(addedEntry, currentUserId);

                return InventoryProductMapper.ToInventoryProductResponseDto(addedEntry);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("UNIQUE constraint failed") == true || ex.InnerException?.Message.Contains("duplicate key") == true)
                {
                    throw new ConflictException($"Product '{product.ProductName}' is already assigned to inventory '{inventory.Name}'. (Database constraint)");
                }
                throw;
            }
        }

        public async Task<InventoryProductResponseDto> IncreaseProductQuantityAsync(AdjustProductQuantityDto dto, int? currentUserId)
        {
            await CheckInventoryManagerAccess(dto.InventoryId, currentUserId);
            var product = await _productRepository.Get(dto.ProductId);
            if (product==null || product.IsDeleted) {
                throw new NotFoundException($"Product ID {dto.ProductId} is not found or is deleted.");
            }
            var inventory = await _inventoryRepository.Get(dto.InventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {dto.InventoryId} not found or is deleted.");
            }
            if (dto.QuantityChange <= 0)
            {
                throw new ArgumentException("QuantityChange must be positive for increasing stock.");
            }

            var entry = await _inventoryProductRepository.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId);
            if (entry == null)
            {
                throw new NotFoundException($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}.");
            }

            await LoadNavigationProperties(entry);

            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(entry));

            entry.Quantity += dto.QuantityChange;
            var updatedEntry = await _inventoryProductRepository.Update(entry.Id, entry);
            
            // AUDIT LOGGING: UPDATE (Increase Quantity)
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "InventoryProducts",
                RecordId = updatedEntry.Id.ToString(),
                ActionType = "QUANTITY_INCREASE",
                OldValues = oldEntrySnapshot,
                NewValues = updatedEntry,
                Changes = $"Increased quantity by {dto.QuantityChange}. From {oldEntrySnapshot.Quantity} to {updatedEntry.Quantity}."
            });

            await CheckAndNotifyLowStock(updatedEntry, currentUserId);

            return InventoryProductMapper.ToInventoryProductResponseDto(updatedEntry);
        }

        public async Task<InventoryProductResponseDto> DecreaseProductQuantityAsync(AdjustProductQuantityDto dto, int? currentUserId)
        {
            await CheckInventoryManagerAccess(dto.InventoryId, currentUserId);
            var product = await _productRepository.Get(dto.ProductId);
            if (product==null || product.IsDeleted) {
                throw new NotFoundException($"Product ID {dto.ProductId} is not found or is deleted.");
            }
            var inventory = await _inventoryRepository.Get(dto.InventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {dto.InventoryId} not found or is deleted.");
            }
            if (dto.QuantityChange <= 0)
            {
                throw new ArgumentException("QuantityChange must be positive for decreasing stock.");
            }

            var entry = await _inventoryProductRepository.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId);
            if (entry == null)
            {
                throw new NotFoundException($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}.");
            }

            await LoadNavigationProperties(entry);

            // Capture old state before modification
            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(entry));

            if (entry.Quantity - dto.QuantityChange < 0)
            {
                throw new InvalidOperationException($"Cannot decrease quantity below zero. Current stock: {entry.Quantity}, requested decrease: {dto.QuantityChange}.");
            }

            entry.Quantity -= dto.QuantityChange;
            var updatedEntry = await _inventoryProductRepository.Update(entry.Id, entry);
            
            // AUDIT LOGGING: UPDATE (Decrease Quantity)
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "InventoryProducts",
                RecordId = updatedEntry.Id.ToString(),
                ActionType = "QUANTITY_DECREASE",
                OldValues = oldEntrySnapshot,
                NewValues = updatedEntry,
                Changes = $"Decreased quantity by {dto.QuantityChange}. From {oldEntrySnapshot.Quantity} to {updatedEntry.Quantity}."
            });

            await CheckAndNotifyLowStock(updatedEntry, currentUserId);

            return InventoryProductMapper.ToInventoryProductResponseDto(updatedEntry);
        }

        public async Task<InventoryProductResponseDto> SetProductQuantityAsync(SetProductQuantityDto dto, int? currentUserId)
        {
            await CheckInventoryManagerAccess(dto.InventoryId, currentUserId);
            var product = await _productRepository.Get(dto.ProductId);
            if (product==null || product.IsDeleted) {
                throw new NotFoundException($"Product ID {dto.ProductId} is not found or is deleted.");
            }
            var inventory = await _inventoryRepository.Get(dto.InventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {dto.InventoryId} not found or is deleted.");
            }
            if (dto.NewQuantity < 0)
            {
                throw new ArgumentException("NewQuantity cannot be negative.");
            }

            var entry = await _inventoryProductRepository.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId);
            if (entry == null)
            {
                throw new NotFoundException($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}. Cannot set quantity.");
            }

            await LoadNavigationProperties(entry);

            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(entry));

            entry.Quantity = dto.NewQuantity;
            var updatedEntry = await _inventoryProductRepository.Update(entry.Id, entry);
            
            // AUDIT LOGGING: UPDATE (Set Quantity)
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "InventoryProducts",
                RecordId = updatedEntry.Id.ToString(),
                ActionType = "QUANTITY_SET",
                OldValues = oldEntrySnapshot,
                NewValues = updatedEntry,
                Changes = $"Set quantity. From {oldEntrySnapshot.Quantity} to {updatedEntry.Quantity}."
            });

            await CheckAndNotifyLowStock(updatedEntry, currentUserId);

            return InventoryProductMapper.ToInventoryProductResponseDto(updatedEntry);
        }

        public async Task<InventoryProductResponseDto> DeleteInventoryProductAsync(int inventoryProductId, int? currentUserId)
        {

            var entryToDelete = await _inventoryProductRepository.Get(inventoryProductId);
            if (entryToDelete == null)
            {
                throw new NotFoundException($"InventoryProduct entry with ID {inventoryProductId} not found.");
            }
            await LoadNavigationProperties(entryToDelete);
            await CheckInventoryManagerAccess(entryToDelete.Inventory.InventoryId, currentUserId);
            // Capture old state before deletion
            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(entryToDelete));

            var deletedEntry = await _inventoryProductRepository.Delete(inventoryProductId);
            
            // AUDIT LOGGING: DELETE
            await _auditLogService.LogActionAsync(new AuditLogEntryDto
            {
                UserId = currentUserId,
                TableName = "InventoryProducts",
                RecordId = deletedEntry.Id.ToString(),
                ActionType = "DELETE",
                OldValues = oldEntrySnapshot,
                NewValues = null // Record is being hard deleted
            });

            return InventoryProductMapper.ToInventoryProductResponseDto(deletedEntry);
        }

        public async Task<InventoryProductResponseDto> UpdateMinStockQuantityAsync(UpdateInventoryProductMinStockDto dto, int? currentUserId)
        {
            await CheckInventoryManagerAccess(dto.InventoryId, currentUserId);
            var product = await _productRepository.Get(dto.ProductId);
            if (product==null || product.IsDeleted) {
                throw new NotFoundException($"Product ID {dto.ProductId} is not found or is deleted.");
            }
            var inventory = await _inventoryRepository.Get(dto.InventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {dto.InventoryId} not found or is deleted.");
            }
            var entryToUpdate = await _inventoryProductRepository.GetByInventoryAndProductId(dto.InventoryId, dto.ProductId);
            if (entryToUpdate == null)
            {
                throw new NotFoundException($"Product ID {dto.ProductId} not found in Inventory ID {dto.InventoryId}. Cannot update min stock  quantity.");
            }

            var oldEntrySnapshot = JsonSerializer.Deserialize<InventoryProduct>(JsonSerializer.Serialize(entryToUpdate));

            entryToUpdate.MinStockQuantity = dto.NewMinStockQuantity;

            try
            {
                var updatedEntry = await _inventoryProductRepository.Update(entryToUpdate.Id, entryToUpdate);

                await _auditLogService.LogActionAsync(new AuditLogEntryDto
                {
                    UserId = currentUserId,
                    TableName = "InventoryProducts",
                    RecordId = updatedEntry.Id.ToString(),
                    ActionType = "MIN_STOCK_UPDATE",
                    OldValues = oldEntrySnapshot,
                    NewValues = updatedEntry,
                    Changes = $"Updated MinStockQuantity from {oldEntrySnapshot.MinStockQuantity} to {updatedEntry.MinStockQuantity}."
                });

                
                await CheckAndNotifyLowStock(updatedEntry, currentUserId);

                return InventoryProductMapper.ToInventoryProductResponseDto(updatedEntry);
            }
            catch (DbUpdateException)
            {
                throw;
            }
        }

        public async Task<InventoryProductResponseDto?> GetInventoryProductByIdAsync(int inventoryProductId)
        {
            var entry = await _inventoryProductRepository.Get(inventoryProductId);
            if (entry == null) return null;
            return InventoryProductMapper.ToInventoryProductResponseDto(entry);
        }

        public async Task<InventoryProductResponseDto?> GetInventoryProductByInventoryAndProductIdAsync(int inventoryId, int productId)
        {
            var entry = await _inventoryProductRepository.GetByInventoryAndProductId(inventoryId, productId);
            if (entry == null) return null;
            return InventoryProductMapper.ToInventoryProductResponseDto(entry);
        }

        public async Task<IEnumerable<InventoryProductResponseDto>> GetAllInventoryProductsAsync(string? sortBy = null)
        {
            var allEntries = await _inventoryProductRepository.GetAll();
            var responseDtos = allEntries.Select(ip => InventoryProductMapper.ToInventoryProductResponseDto(ip));

            if (!string.IsNullOrEmpty(sortBy))
            {
                responseDtos = SortHelper.ApplySorting(responseDtos, sortBy);
            }
            return responseDtos;
        }

        public async Task<IEnumerable<ProductInInventoryResponseDto>> GetProductsInInventoryAsync(int inventoryId, string? sortBy = null)
        {
            var inventory = await _inventoryRepository.Get(inventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found or is deleted.");
            }

            var entries = await _inventoryProductRepository.GetProductsForInventory(inventoryId);
            var activeProducts = entries.Where(ip => ip.Product != null && !ip.Product.IsDeleted)
                                         .Select(ip => InventoryProductMapper.ToProductInInventoryResponseDto(ip));

            if (!string.IsNullOrEmpty(sortBy))
            {
                activeProducts = SortHelper.ApplySorting(activeProducts, sortBy);
            }
            return activeProducts;
        }

        public async Task<PaginationResponse<ProductInInventoryResponseDto>> GetProductsInInventoryAsync(
            int inventoryId,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null)
        {
            var inventory = await _inventoryRepository.Get(inventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found or is deleted.");
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);


            IQueryable<InventoryProduct> query = _inventoryProductRepository.GetProductsForInventoryAsQueryable(inventoryId);

            query = query.Where(ip => ip.Product != null && !ip.Product.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(ip => ip.Product.ProductName.ToLower().Contains(searchTerm)||ip.Product.SKU.ToLower().Contains(searchTerm) || 
                                          ip.Product.Category.CategoryName.ToLower().Contains(searchTerm));
            }

            int totalRecords = await query.CountAsync();


            query = query.ApplyDatabaseSorting(orderBy, "Id");

            var inventoryProducts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var inventoryProductResponseDtos = inventoryProducts.Select(ip => InventoryProductMapper.ToProductInInventoryResponseDto(ip));

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var paginationMetadata = new PaginationMetadata
            {
                TotalRecords = totalRecords,
                Page = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new PaginationResponse<ProductInInventoryResponseDto>
            {
                Data = inventoryProductResponseDtos,
                Pagination = paginationMetadata
            };
        }

        public async Task<IEnumerable<ProductInInventoryResponseDto>> GetProductsInInventoryByCategoryAsync(int inventoryId, int categoryId, string? sortBy = null)
        {
            var inventory = await _inventoryRepository.Get(inventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found or is deleted.");
            }
            var category = await _categoryRepository.Get(categoryId);
            if (category == null)
            {
                throw new NotFoundException($"Category with ID {categoryId} not found.");
            }

            var entries = await _inventoryProductRepository.GetProductsInInventoryByCategory(inventoryId, categoryId);
            var filteredProducts = entries.Where(ip => ip.Product != null && !ip.Product.IsDeleted)
                                          .Select(ip => InventoryProductMapper.ToProductInInventoryResponseDto(ip));

            if (!string.IsNullOrEmpty(sortBy))
            {
                filteredProducts = SortHelper.ApplySorting(filteredProducts, sortBy);
            }
            return filteredProducts;
        }

        public async Task<IEnumerable<InventoryForProductResponseDto>> GetInventoriesForProductAsync(int productId, string? sortBy = null)
        {
            var product = await _productRepository.Get(productId);
            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with ID {productId} not found or is deleted.");
            }

            var entries = await _inventoryProductRepository.GetInventoriesForProduct(productId);
            var activeInventories = entries.Where(ip => ip.Inventory != null && !ip.Inventory.IsDeleted)
                                           .Select(ip => InventoryProductMapper.ToInventoryForProductResponseDto(ip));

            if (!string.IsNullOrEmpty(sortBy))
            {
                activeInventories = SortHelper.ApplySorting(activeInventories, sortBy);
            }
            return activeInventories;
        }

        public async Task<PaginationResponse<InventoryForProductResponseDto>> GetInventoriesForProductAsync(
            int productId,
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            string? orderBy = null)
        {
            var product = await _productRepository.Get(productId);
            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with ID {productId} not found or is deleted.");
            }

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);


            IQueryable<InventoryProduct> query = _inventoryProductRepository.GetInventoriesForProductAsQueryable(productId);

            query = query.Where(ip => ip.Inventory != null && !ip.Inventory.IsDeleted);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(ip => ip.Inventory.Name.ToLower().Contains(searchTerm));
            }

            int totalRecords = await query.CountAsync();


            query = query.ApplyDatabaseSorting(orderBy, "Id");

            var inventoryProducts = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var inventoryProductResponseDtos = inventoryProducts.Select(ip => InventoryProductMapper.ToInventoryForProductResponseDto(ip));

            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var paginationMetadata = new PaginationMetadata
            {
                TotalRecords = totalRecords,
                Page = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };

            return new PaginationResponse<InventoryForProductResponseDto>
            {
                Data = inventoryProductResponseDtos,
                Pagination = paginationMetadata
            };
        }

        public async Task<IEnumerable<InventoryForProductResponseDto>> GetInventoriesForProductBySKUAsync(string sku, string? sortBy = null)
        {
            var product = await _productRepository.GetBySKU(sku);
            if (product == null || product.IsDeleted)
            {
                throw new NotFoundException($"Product with SKU '{sku}' not found or is deleted.");
            }

            return await GetInventoriesForProductAsync(product.ProductId, sortBy);
        }

        public async Task<IEnumerable<ProductInInventoryResponseDto>> GetLowStockProductsInInventoryAsync(int inventoryId, int threshold, string? sortBy = null)
        {
            var inventory = await _inventoryRepository.Get(inventoryId);
            if (inventory == null || inventory.IsDeleted)
            {
                throw new NotFoundException($"Inventory with ID {inventoryId} not found or is deleted.");
            }

            var entries = await _inventoryProductRepository.GetLowStockProducts(inventoryId, threshold);
            var lowStockProducts = entries.Where(ip => ip.Product != null && !ip.Product.IsDeleted)
                                          .Select(ip => InventoryProductMapper.ToProductInInventoryResponseDto(ip));

            if (!string.IsNullOrEmpty(sortBy))
            {
                lowStockProducts = SortHelper.ApplySorting(lowStockProducts, sortBy);
            }
            return lowStockProducts;
        }
    }
}
