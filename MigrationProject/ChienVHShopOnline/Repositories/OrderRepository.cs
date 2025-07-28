using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class OrderRepository : Repository<int, Order>
    {
        public OrderRepository(ApplicationDbContext context) : base(context) {}

        public override async Task<Order?> Get(int key)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderID == key);
        }

        public override async Task<IEnumerable<Order>> GetAll()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ToListAsync();
        }
    }
}
