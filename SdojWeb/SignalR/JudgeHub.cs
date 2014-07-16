using Microsoft.AspNet.SignalR;

namespace SdojWeb.SignalR
{
    public class JudgeHub : Hub
    {
        public void Send(string text)
        {
            var user = Context.User;
            Clients.All.DoWork(text);
        }
    }
}