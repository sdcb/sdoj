using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System.Web.ClientServices;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Security;
using SdojWeb.Models;

namespace SdojWeb.Controllers
{
    public class JudgeConnection : PersistentConnection
    {
        public ApplicationUserManager UserManager
        {
            get { return DependencyResolver.Current.GetService<ApplicationUserManager>(); }
        }

        private static readonly AutoResetEvent RunOnce = new AutoResetEvent(true);

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            if (RunOnce.WaitOne(0))
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        Connection.Broadcast(DateTime.Now.ToString("O"));
                        Thread.Sleep(1000);
                    }
                });
            }
            return Connection.Send(connectionId, "Welcome!");
        }

        protected override bool AuthorizeRequest(IRequest request)
        {
            var username = request.Headers["username"];
            var password = request.Headers["password"];
            var user = UserManager.Find(username, password);
            
            if (user != null)
            {
                return true;
            }
            return false;
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