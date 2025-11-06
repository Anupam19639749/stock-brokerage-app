using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StockAlertTracker.API.Hubs
{
    [Authorize] // Only authenticated users can connect to this hub
    public class PriceHub : Hub
    {
        // This hub is a "broadcast" hub.
        // The server will push messages to clients.
        // Clients don't send messages to the server (for this feature).

        // We can add logic here if we want to group users by ID,
        // but for a global broadcast, this empty class is all we need.
    }
}