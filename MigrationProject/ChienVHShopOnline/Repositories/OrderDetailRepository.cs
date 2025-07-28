using ChienVHShopOnline.Data;
using ChienVHShopOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace ChienVHShopOnline.Repositories
{
    public class OrderDetailRepository : Repository<(int OrderId, int ProductId), OrderDetail>
    {
        public OrderDetailRepository(ApplicationDbContext context) : base(context) {}

        public override async Task<OrderDetail?> Get((int OrderId, int ProductId) key)
        {
            return await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .FirstOrDefaultAsync(od => od.OrderID == key.OrderId && od.ProductID == key.ProductId);
        }

        public override async Task<IEnumerable<OrderDetail>> GetAll()
        {
            return await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .ToListAsync();
        }
    }
}
