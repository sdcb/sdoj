using Microsoft.AspNet.SignalR;
using SdojWeb.Infrastructure.Extensions;
using SdojWeb.Models;

namespace SdojWeb.SignalR
{
    public class SolutionHub : Hub
    {
        public static void PushChange(int solutionId, SolutionState state, int timeMs, float memoryMb)
        {
            var hub = GlobalHost.ConnectionManager.GetHubContext<SolutionHub>();
            hub.Clients.All.push(solutionId, state.GetDisplayName(), timeMs, memoryMb);
        }
    }
}