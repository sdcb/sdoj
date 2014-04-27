using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SdojWeb.Controllers
{
    public class JudgeConnection : PersistentConnection
    {
        protected override Task OnConnected(IRequest request, string connectionId)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    await Connection.Send(connectionId, DateTime.Now.ToString());
                }
            });
            return Connection.Send(connectionId, "Welcome!");
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            return base.AuthorizeRequest(request);
        }

        protected override Task OnDisconnected(IRequest request, string connectionId)
        {
            
            return base.OnDisconnected(request, connectionId);
        }

        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }
    }
}