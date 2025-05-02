using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeatureObjects.Abstraction.IManager
{
    public interface INotificationService
    {
        Task SendStatusUpdateasync(string message, string status = "info");


        Task SendProgressAsync(string message);


    }
}
