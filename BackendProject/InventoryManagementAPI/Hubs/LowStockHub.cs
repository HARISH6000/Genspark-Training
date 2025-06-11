using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Hubs
{
    
    public class LowStockHub : Hub
    {
        // No specific methods here yet, as we will call client-side methods directly from the service.
        // You could add methods here if clients needed to invoke server-side actions related to stock.
    }
}
