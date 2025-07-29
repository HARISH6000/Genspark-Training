using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderReadDto>> GetAllAsync();
    Task<OrderReadDto?> GetByIdAsync(int id);
    Task<byte[]?> ExportOrderListingPdfAsync();
}
