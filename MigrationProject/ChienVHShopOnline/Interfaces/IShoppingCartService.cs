using ChienVHShopOnline.DTOs;

namespace ChienVHShopOnline.Interfaces;

public interface IShoppingCartService
{
    Task<OrderResponseDto> ProcessOrderAsync(OrderRequestDto request);
}
