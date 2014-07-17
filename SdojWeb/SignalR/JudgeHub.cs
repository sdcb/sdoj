using Microsoft.AspNet.SignalR;
using SdojWeb.Infrastructure.Identity;

namespace SdojWeb.SignalR
{
    [Authorize(Roles = SystemRoles.Judger)]
    public class JudgeHub : Hub
    {
        public void Send(string text)
        {
            var user = Context.User;
            Clients.All.DoWork(text);
        }
    }
}