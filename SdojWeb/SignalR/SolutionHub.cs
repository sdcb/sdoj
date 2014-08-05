using Microsoft.AspNet.SignalR;

namespace SdojWeb.SignalR
{
    public class SolutionHub : Hub
    {
        public static void PushChange(int solutionId, string stateName)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<SolutionHub>();
            hub.Clients.All.push(solutionId, stateName) ;
        }
    }
}