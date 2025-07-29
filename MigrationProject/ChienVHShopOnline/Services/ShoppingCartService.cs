using ChienVHShopOnline.Data;
using ChienVHShopOnline.DTOs;
using ChienVHShopOnline.Interfaces;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly ApplicationDbContext _context;

    public ShoppingCartService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponseDto> ProcessOrderAsync(OrderRequestDto request)
    {
        var user = await _context.Users.FindAsync(request.UserId);
        if (user == null)
            throw new Exception("User not found");

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new Exception("One or more products not found");

        var order = new Order
        {
            OrderName = $"Order_{DateTime.UtcNow.Ticks}",
            CustomerName = user.Username,
            CustomerPhone = request.CustomerPhone,
            CustomerEmail = request.CustomerEmail,
            CustomerAddress = request.CustomerAddress,
            OrderDate = DateTime.UtcNow,
            PaymentType = "Cash",
            Status = "Processing"
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        foreach (var item in request.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);
            var orderDetail = new OrderDetail
            {
                OrderID = order.OrderID,
                ProductID = product.ProductId,
                Quantity = item.Quantity,
                Price = product.Price ?? 0
            };
            _context.OrderDetails.Add(orderDetail);
        }

        await _context.SaveChangesAsync();

        return new OrderResponseDto
        {
            OrderId = order.OrderID,
            Status = order.Status
        };
    }
}
