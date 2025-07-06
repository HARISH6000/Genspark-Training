using InventoryManagementAPI.DTOs;

namespace InventoryManagementAPI.DTOs
{
    public class PaginationResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public PaginationMetadata? Pagination { get; set; }
    }
}