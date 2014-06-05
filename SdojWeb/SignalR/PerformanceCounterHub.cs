using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace SdojWeb.SignalR
{
    [HubName("pcHub")]
    public class PerformanceCounterHub : Hub
    {
        public static bool Started = false;

        public override Task OnConnected()
        {
            if (!Started)
            {
                Started = true;

                Task.Run(async () =>
                {
                    var ps = Process.GetCurrentProcess();
                    while (true)
                    {
                        Clients.All.pc(
                            ps.TotalProcessorTime.ToString(@"hh\:mm\:ss"),
                            (DateTime.Now - ps.StartTime).ToString(@"hh\:mm\:ss"),
                            ps.Threads.Count,
                            GC.GetTotalMemory(false));
                        await Task.Delay(1000);
                    }
                });
            }
            return base.OnConnected();
        }
    }
}