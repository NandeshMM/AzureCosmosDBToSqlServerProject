using FeatureObjects.Abstraction.IManager;
using Microsoft.AspNetCore.SignalR;
using static AzureCosmosDBToSqlServerProject.NotificationCenter.NotificationService;

namespace AzureCosmosDBToSqlServerProject.NotificationCenter
{
    public class NotificationService: INotificationService
    {

            private readonly IHubContext<NotificationHub> _hubContext;

            public NotificationService(IHubContext<NotificationHub> hubContext)
            {
                _hubContext = hubContext;
            }

            public async Task SendStatusUpdateasync(string message, string status = "info")
            {

                var statuspayload = new
                {
                    message,
                    status
                };
                await _hubContext.Clients.All.SendAsync("ReceivedstatusUpdate", statuspayload);
            }
            public async Task SendProgressAsync(string message)
            {
                await _hubContext.Clients.All.SendAsync("ReceivedProgress", message);
            }
    }
}
