using Microsoft.AspNetCore.SignalR;

namespace AzureCosmosDBToSqlServerProject.NotificationCenter
{
    public class NotificationHub:Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine("client connected");
        }
 
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine("client disconnected");
            return base.OnDisconnectedAsync(exception);
        }
    }
}
