using Microsoft.AspNetCore.SignalR;

namespace NotifyAPI.Misc
{
    public class DocumentHub : Hub
    {
        public async Task NotifyNewDocument(string fileName)
        {
            await Clients.All.SendAsync("DocumentAdded", fileName);
        }
    }
}
